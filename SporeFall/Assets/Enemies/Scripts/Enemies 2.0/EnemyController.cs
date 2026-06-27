using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] EnemyStatSO statData;
    public List<Attack> attackSlots = new();

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float targetRescanRate = 1f;
    private float _rescanTimer;
    [Tooltip("Hysteresis multiplier: enemy re-enters Moving only when distance exceeds " +
                "the eligible attack range by this factor. Prevents jitter at range boundaries.")]
    public float attackRangeHysteresis = 1.15f;
    // AI
    public EnemyStatBlock Stats { get; private set; } = new();
    private NavMeshAgent _agent;
    private EnemyState _state;
    public Action<EnemyState> OnStateChange;

    // Attacking
    private Damageable _target;
    private List<AttackInstance> _attacks = new();
    private AttackInstance _activeAttack;   // currently executing (null = on cooldown/idle)
    private float _attackCooldown; // time remaining before next selection

    [Header("References")]
    [SerializeField] protected Damageable health;
    public EnemyAnimator EnemyAnimator { get; private set; }
    public AudioSource AudioSource => audioSource;
    // SFX
    protected AudioSource audioSource;

    // Testing Variables
    [Header("For Testing Purposes")]
    public bool AutoInitialize = false;
    public int initialLevel = 0;
    public TMP_Text testLvDisplay;
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        EnemyAnimator = GetComponent<EnemyAnimator>();

        if (AutoInitialize)
        {
            Initialize(initialLevel);
        }
    }
    public void Initialize(int level)
    {
        Stats.Apply(statData, level);
        
        _agent.speed = Stats.MoveSpeed;
        health.maxHealth = Stats.MaxHealth;
        health.SetDamageReduction(Stats.Armor);
        health.ResetHealth();


        _attacks.Clear();
        foreach(var attackData in attackSlots)
        {
            if(attackData == null) continue;
            var attack = new AttackInstance();
            attack.Initialize(attackData, this, level, statData.damageScale);
            _attacks.Add(attack);
        }

        _state = EnemyState.Searching;
        _rescanTimer = 0;
        _target = null;
        _activeAttack = null;
        _attackCooldown = 0f;

        testLvDisplay.text = "LV: " + level.ToString();
    }
    public void ResetForPool()
    {
        StopAllCoroutines();

        health.ResetHealth();
        ReleaseCurrentToken();

        Stats.Reset();
        foreach(var atk in _attacks)
        {
            atk.Reset();
        }
        _attacks.Clear();

        if(_agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.ResetPath();
        }

        _state = EnemyState.Idle;
        _target = null;
        _activeAttack = null;
        _attackCooldown = 0f;
    }
    private void Update()
    {
        switch(_state)
        {
            case EnemyState.Searching:
                TickSearching();
                break;
            case EnemyState.Moving:
                TickMoving();
                break;
            case EnemyState.AtTarget:
                TickMoving();
                break;
            case EnemyState.Attacking:
                TickAttacking();
                break;
            case EnemyState.Retreating:
                TickRetreating();
                break;
            case EnemyState.Dead:
                break;

            default:
                break;
        }
    }
    private void TickSearching()
    {
        _rescanTimer -= Time.deltaTime;
        if (_rescanTimer > 0f) return;
        
        _rescanTimer = targetRescanRate;

        Damageable found = EnemyTargetRegistry.Instance?.FindBestTarget(

            transform.position, detectionRange,
            gameObject.GetInstanceID(),
            statData.targetPriority, _target
            );

        if (found == null || found.targetType == TargetType.Enemy) return;

        SetTarget(found);
        TransitionTo(EnemyState.Moving);
    }
    private void TickMoving()
    {
        if(_target == null || !_target.IsAlive)
        {
            ReleaseCurrentToken();
            TransitionTo(EnemyState.Searching);
            return;
        }

        // look for better target 
        _rescanTimer -= Time.deltaTime;
        if(_rescanTimer <= 0f)
        {
            _rescanTimer = targetRescanRate;
            Damageable better = EnemyTargetRegistry.Instance?.FindBestTarget(
            
                    transform.position, detectionRange,
                    gameObject.GetInstanceID(),
                    statData.targetPriority, _target
            );

            if(better != null && better != _target)
                SetTarget(better);

            // Stop and attack as soon as ANY eligible attack can reach the target
            float dist = Vector3.Distance(transform.position, _target.transform.position);

            if(dist <= _agent.stoppingDistance)
            {
                TransitionTo(EnemyState.AtTarget);
            }    

            Debug.Log($"Current Distance To Target: {dist:F0}");
            if (HasAnyAttackInRange(dist))
            {
                _agent.isStopped = true;
                Debug.Log("Changing To Attacking");
                TransitionTo(EnemyState.Attacking);
            }
        }

        UpdateNavDestination();
    }
    private void TickRetreating()
    {

    }
    private void TickAttacking()
    {
        if (_target == null || !_target.IsAlive)
        {
            _agent.isStopped = false;
            ReleaseCurrentToken();
            _activeAttack = null;
            _attackCooldown = 0f;
            TransitionTo(EnemyState.Searching);
            return;
        }

        // Face target
        Vector3 dir = (_target.transform.position - transform.position);
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, Quaternion.LookRotation(dir), 360f * Time.deltaTime);

        // Check target hasn't moved out of attack range
        float distToTarget = dir.magnitude;

        if (!HasAnyAttackInRange(distToTarget * (1f / attackRangeHysteresis)))
        {
            _agent.isStopped = false;
            _activeAttack = null;
            _attackCooldown = 0f;
            TransitionTo(EnemyState.Moving);
            return;
        }

        // wait for attack cooldown
        if (_attackCooldown > 0f)
        {
            _attackCooldown -= Time.deltaTime;
            return;   // waiting — do nothing this frame
        }

        // Select an Attack

        _activeAttack = SelectAttack(distToTarget);
        if (_activeAttack == null)
        {
            // Edge case: in range overall but no single attack fits right now.
            _attackCooldown = 0.1f;
            return;
        }

        if( _activeAttack != null && _activeAttack.MinSelectRange > distToTarget)
        {
            // fallback to execute attack
            TransitionTo(EnemyState.Retreating);
            return;
        }

       StartCoroutine(ExecuteAttack());
       
    }
    protected IEnumerator ExecuteAttack()
    {
        TransitionTo(EnemyState.ExecutingAttack);

        EnemyAnimator.Animator.SetTrigger(_activeAttack.Data.animationTrigger);

        yield return new WaitForSeconds(_activeAttack.Data.attackDelay);
        // execute Attack
        _activeAttack.Execute(_target);
        Debug.Log($"Executing Attack: {_activeAttack.Data.attackName}");

        yield return new WaitForSeconds(_activeAttack.Data.recoveryTime);

        _activeAttack.SetLastUseTime(Time.time);
        
        _attackCooldown = 1f;
        _agent.isStopped = false;
        _activeAttack = null;
        TransitionTo(EnemyState.Attacking);
    }
    private AttackInstance SelectAttack(float distToTarget)
    {
        float totalWeight = 0f;
        for (int i = 0; i < _attacks.Count; i++)
        {
            var a = _attacks[i];
            if (distToTarget >= a.MinSelectRange && distToTarget <= a.AttackRange)
                totalWeight += a.SelectionWeight;
        }

        if (totalWeight <= 0f) return null;

        float roll = UnityEngine.Random.value * totalWeight;
        float acc = 0f;

        for (int i = 0; i < _attacks.Count; i++)
        {
            var a = _attacks[i];

            if (distToTarget > a.AttackRange || !a.CanUse()) continue;
            acc += a.SelectionWeight;
            if (roll <= acc) return a;
        }

        return null;  // shouldn't reach here, but safe fallback
    }
    private bool HasAnyAttackInRange(float dist)
    {
        Debug.Log("Searching Through Attacks");
        for (int i = 0; i < _attacks.Count; i++)
        {
            var a = _attacks[i];
            if (dist >= a.MinSelectRange && dist <= a.AttackRange && a.CanUse())
                return true;
        }
        return false;
    }

    #region Targeting and Tokens
    private void SetTarget(Damageable newTarget)
    {
        if (newTarget == _target) return;
        
        ReleaseCurrentToken();

        if (newTarget.TryAcquireToken(gameObject.GetInstanceID()))
        {
            _target = newTarget;
            UpdateNavDestination(force: true);
        }
        else
        {
            // Token denied — keep searching
            _target = null;
            TransitionTo(EnemyState.Searching);
        }
    }
    public void ReleaseCurrentToken()
    {
        _target?.ReleaseToken(gameObject.GetInstanceID());
    }
    #endregion
    private Vector3 _lastNavTarget;
    private const float NAV_UPDATE_THRESHOLD_SQ = 1f; // 1m² before re-pathing
    private void UpdateNavDestination(bool force = false)
    {
        if (_target == null) return;
        Vector3 dest = _target.transform.position;
        if (!force && (dest - _lastNavTarget).sqrMagnitude < NAV_UPDATE_THRESHOLD_SQ)
            return;

        _lastNavTarget = dest;
        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            _agent.SetDestination(dest);
    }
    public void TransitionTo(EnemyState next)
    {
        _state = next;
        OnStateChange?.Invoke(next);
    }
    public EnemyState CurrentState => _state;
}

public enum EnemyState
{
   Idle,
   Searching,
   Moving,
   AtTarget,
   Attacking,
   ExecutingAttack,
   Retreating,
   Dead
}