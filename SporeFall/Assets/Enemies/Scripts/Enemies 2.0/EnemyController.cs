using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

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

    [Header("Repositioning")]
    [Tooltip("How close to the preferred range the enemy must get before re-attacking.")]
    public float repositionTolerance = 0.4f;
    [Tooltip("Chance (0–1) the enemy actually repositions after an attack. " +
             "0 = never reposition, 1 = always reposition.")]
    [Range(0f, 1f)]
    public float repositionChance = 0.6f;


    // Attacking
    private Damageable _target;
    private List<AttackInstance> _attacks = new();
    private AttackInstance _activeAttack;   // currently executing (null = on cooldown/idle)
    private float _attackCooldown; // time remaining before next selection

    private MovementAttackDriver _movDriver;    // optional, may be null
    private float _repoTargetDist;   // preferred range of the last attack fired



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
    public TMP_Text stateDisplay;


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        EnemyAnimator = GetComponent<EnemyAnimator>();
        _movDriver = GetComponent<MovementAttackDriver>();    // null until first movement attack
    }

    private void OnEnable()
    {
        // For testing Only
        Initialize(initialLevel);
    }
    public void Initialize(int level)
    {
        Stats.Apply(statData, level);
        
        _agent.speed = Stats.MoveSpeed;
        health.maxHealth = Stats.MaxHealth;
        health.SetDamageReduction(Stats.Armor);
        health.MakeAlive();


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
        _repoTargetDist = 0f;

        // remove later
        testLvDisplay.text = "LV: " + level.ToString();
    }
    public void ResetForPool()
    {
        StopAllCoroutines();
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
            _agent.isStopped = false;
            _agent.velocity = Vector3.zero;
        }

        _state = EnemyState.Idle;
        _target = null;
        _activeAttack = null;
        _attackCooldown = 0f;
        _repoTargetDist = 0f;

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
                TryStartRepositioning();
                break;
            case EnemyState.Attacking:
                TickAttacking();
                break;
            case EnemyState.Repositioning:
                TickRepositioning();
                break;
            case EnemyState.Defending:
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

        // Stop and attack as soon as ANY eligible attack can reach the target
        float dist = Vector3.Distance(transform.position, _target.transform.position);

        if (dist <= _agent.stoppingDistance)
        {
            TransitionTo(EnemyState.AtTarget);
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

            if (HasAnyAttackInRange(dist))
            {
                _agent.isStopped = true;
                TransitionTo(EnemyState.Attacking);
            }
        }

        UpdateNavDestination();
    }
    private void TickRepositioning()
    {
        if (_target == null || !_target.IsAlive)
        {
            ReleaseCurrentToken();
            _agent.isStopped = false;
            TransitionTo(EnemyState.Searching);
            return;
        }

        // Advance cooldown while repositioning
        if (_attackCooldown > 0f)
            _attackCooldown -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, _target.transform.position);

        // Reached preferred range → stop and wait for cooldown or attack
        if (Mathf.Abs(dist - _repoTargetDist) <= repositionTolerance)
        {
            _agent.isStopped = true;

            if (_attackCooldown <= 0f)
                TransitionTo(EnemyState.Attacking);
            // else stay here, cooldown finishes, next Update will re-evaluate
            return;
        }

        // Set nav destination offset from target at preferred range
        Vector3 awayDir = (transform.position - _target.transform.position).normalized;
        Vector3 repoPos = _target.transform.position + awayDir * _repoTargetDist;
        _agent.isStopped = false;
        SetNavDestinationDirect(repoPos);

        _rescanTimer -= Time.deltaTime;
        if (_rescanTimer <= 0f)
        {
            _rescanTimer = targetRescanRate;
            // If we've finished the cooldown but not yet reached range, just attack anyway
            if (_attackCooldown <= 0f && HasAnyAttackInRange(dist))
            {
                _agent.isStopped = true;
                TransitionTo(EnemyState.Attacking);
            }
        }
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

            TryStartRepositioning();
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
            _attackCooldown = 0.1f;
            return;
        }

        if( _activeAttack != null && _activeAttack.MinSelectRange > distToTarget)
        {
            // fallback to execute attack
            TransitionTo(EnemyState.Repositioning);
            return;
        }

       StartCoroutine(ExecuteAttack());
       
    }
    protected IEnumerator ExecuteAttack()
    {
        TransitionTo(EnemyState.ExecutingAttack);

        EnemyAnimator.Animator.SetTrigger(_activeAttack.Data.animationTrigger);

        yield return new WaitForSeconds(_activeAttack.Data.attackDelay);

        Debug.Log($"Executing Attack: {_activeAttack.Data.attackName}");

        if (_activeAttack.Data.AttackType == AttackType.MovementAttack)
        {
            // Hand off to the movement attack driver
            _activeAttack.Execute(_target);

            // Cache driver reference if it was just added by Execute()
            if (_movDriver == null)
                _movDriver = GetComponent<MovementAttackDriver>();

            /*if (_movDriver != null)
                _movDriver.OnCompleted += OnMovementAttackCompleted;*/

            TransitionTo(EnemyState.ExecutingMovementAttack);

            yield return new WaitForSeconds(_activeAttack.Data.recoveryTime);

            OnMovementAttackCompleted();
        }
        else
        {
            // execute Attack
            _activeAttack.Execute(_target);

            yield return new WaitForSeconds(_activeAttack.Data.recoveryTime);

            _activeAttack.SetLastUseTime(Time.time);
            _repoTargetDist = _activeAttack.Data.preferredRange;

            _attackCooldown = 1f;
            _agent.isStopped = false;
            _activeAttack = null;
            TryStartRepositioning();
            TransitionTo(EnemyState.Attacking);
        }
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

    private void OnMovementAttackCompleted()
    {
        /*if (_movDriver != null)
            _movDriver.OnCompleted -= OnMovementAttackCompleted;*/

        _activeAttack.SetLastUseTime(Time.time);
        _repoTargetDist = _activeAttack.Data.preferredRange;

        _attackCooldown = 1f;
        _agent.isStopped = false;
        _activeAttack = null;

        if (_state == EnemyState.Dead) return;
        TryStartRepositioning();
        TransitionTo(EnemyState.Attacking);
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
    private void SetNavDestinationDirect(Vector3 dest, bool force = false)
    {
        if (!force && (dest - _lastNavTarget).sqrMagnitude < NAV_UPDATE_THRESHOLD_SQ) return;
        _lastNavTarget = dest;
        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            _agent.SetDestination(dest);
    }
    public void TransitionTo(EnemyState next)
    {
        _state = next;
        OnStateChange?.Invoke(next);

        stateDisplay.text = _state.ToString();
    }
    private void TryStartRepositioning()
    {
        // Skip repositioning if no preferred range, chance fails, or target is dead
        if (_repoTargetDist <= 0f || _target == null || !_target.IsAlive) return;
        if (UnityEngine.Random.value > repositionChance) return;

        float dist = Vector3.Distance(transform.position, _target.transform.position);
        if (Mathf.Abs(dist - _repoTargetDist) <= repositionTolerance) return; // already there

        _agent.isStopped = false;
        TransitionTo(EnemyState.Repositioning);
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
   Repositioning,
   ExecutingMovementAttack,
   Defending,
   Dead
}