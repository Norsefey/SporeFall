using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BossState
{
    Pursuing,
    Attacking,
    Repositioning
}

// Abstract base class for all bosses
public abstract class BaseBoss : MonoBehaviour
{
    [Header("Base Components")]
    [SerializeField] protected Attack[] attacks;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected AudioSource audioSource;
    protected Damageable health;
    public Transform firePoint;
    [Header("Attack Stats")]
    [SerializeField] protected float targetSwitchThreshold = 100f;
    [SerializeField] protected float minApproachDis = 20f;
    protected Transform currentTarget;
    protected Transform player;
    protected Transform payload;
    protected float damageTaken;
    [SerializeField] protected float minAttackInterval = 5;
    [SerializeField] protected float maxAttackInterval = 8;
    private float intervalCooldown;
    protected bool isAttacking;

    public Animator Animator => animator;
    public AudioSource AudioSource => audioSource;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        health = GetComponent<Damageable>();

        foreach (var att in attacks)
        {// scriptable objects are funky, need to manually reset last use time on all attacks, otherwise it stores time from previous plays
            att.ResetCooldown();
        }

        currentTarget = payload;
        intervalCooldown = Random.Range(minAttackInterval, maxAttackInterval);
    }
    protected virtual void Update()
    {
        if (!isAttacking)
        {
            UpdateState();
        }
    }
    public void SetIsAttacking(bool attacking)
    {
        isAttacking = attacking;
        if (attacking)
        {
            agent.isStopped = true;
        }
        else
        {
            intervalCooldown = Random.Range(minAttackInterval, maxAttackInterval);
            agent.isStopped = false;
        }
    }
    protected virtual void UpdateState()
    {
        // Update target based on damage taken
        if (damageTaken >= targetSwitchThreshold && currentTarget != player)
        {
            currentTarget = player;
        }

        intervalCooldown -= Time.deltaTime * 1;
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (!isAttacking && intervalCooldown <= 0)
        {
            Debug.Log("Distance: " + distanceToTarget);
            Debug.Log("Trying to Attack");
            Attack bestAttack = ChooseBestAttack(distanceToTarget);
            if (bestAttack != null)
            {
                Debug.Log("Attacking");

                StartCoroutine(bestAttack.ExecuteAttack(this, currentTarget));
                return;
            }
            else
            {
                Debug.Log("Cannot Attack");
                agent.isStopped = false;
                intervalCooldown = Random.Range(minAttackInterval, maxAttackInterval);
            }
        }else if (distanceToTarget > minApproachDis)
        {
            agent.SetDestination(currentTarget.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }
    protected virtual Attack ChooseBestAttack(float distanceToTarget)
    {
        Attack bestAttack = null;
        float bestPriority = float.MinValue;

        foreach (Attack attack in attacks)
        {
            if (attack.CanUse(distanceToTarget))
            {
                float priority = EvaluateAttackPriority(attack, distanceToTarget);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestAttack = attack;
                }
            }
            else
            {
                Debug.Log("Cannnot Use Attack: " + attack.name);
            }
        }

        return bestAttack;
    }
    protected virtual float EvaluateAttackPriority(Attack attack, float distanceToTarget)
    {
        // Override this to implement custom priority logic
        return 1f;
    }
    public void StartDOTEffect(Damageable target, float totalDamage, float duration, float tickRate)
    {
        StartCoroutine(ApplyDOTDamage(target, totalDamage, duration, tickRate));
    }
    private IEnumerator ApplyDOTDamage(Damageable target, float totalDamage, float duration, float tickRate)
    {
        float elapsedTime = 0f;
        float damagePerTick = totalDamage * (tickRate / duration);

        while (elapsedTime < duration)
        {
            target.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(tickRate);
            elapsedTime += tickRate;
        }
    }
    public void AssignReferences(Transform payload, Transform player)
    {
        this.payload = payload;
        this.player = player;
    }
    protected virtual void Die()
    {
        // Implement death behavior
        Destroy(gameObject);
    }
}
