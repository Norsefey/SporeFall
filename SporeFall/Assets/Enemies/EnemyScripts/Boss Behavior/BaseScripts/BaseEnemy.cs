using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;


public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Retreat,
    Strafe
}

// Abstract base class for all bosses
public abstract class BaseEnemy : MonoBehaviour
{
    // Public events
    public delegate void EnemyDeath();
    public event EnemyDeath OnEnemyDeath;

    [Header("Base Components")]
    [SerializeField] protected Attack[] attacks;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected AudioSource audioSource;
    protected Damageable health;
    public Transform firePoint;

    [Header("Strafe Behavior")]
    [SerializeField] protected float strafeRadius = 10f;
    [SerializeField] protected float retreatDistance = 15f;
    [SerializeField] protected float strafeSpeed = 5f;
    protected EnemyState currentState = EnemyState.Idle;
    private float stateTimer;
    private Vector3 strafeTarget;
    private bool strafeDirectionRight = true;
    [Header("State Selection")]
    [SerializeField] protected float chasePriorityDistance = 25f; // Distance above which chase becomes high priority
    [SerializeField] protected float damagePriorityThreshold = 20f; // Amount of damage that triggers defensive priorities
    [SerializeField] protected float damageTrackingDuration = 3f; // How long to track damage for
    private bool targetingStructure = false;
    public Queue<DamageInstance> recentDamage = new Queue<DamageInstance>();
    public struct DamageInstance
    {
        public float amount;
        public float time;

        public DamageInstance(float amount, float time)
        {
            this.amount = amount;
            this.time = time;
        }
    }
    protected struct StateWeight
    {
        public EnemyState state;
        public float weight;

        public StateWeight(EnemyState state, float weight)
        {
            this.state = state;
            this.weight = weight;
        }
    }
    [Header("Attack Stats")]
    [SerializeField] protected float stoppingDistance = 20f;
    [SerializeField] protected float minAttackInterval = 5;
    [SerializeField] protected float maxAttackInterval = 8;
    [SerializeField] protected float aggressionFactor = 0.6f; // Chance to choose aggressive actions
    protected bool isAttacking;

    [Header("Targeting")]
    public TrainHandler train; // if nothing is in range will move to Payload or train
    public Transform currentTarget;
    // Array to hold multiple valid target tags
    public string[] priorityTags; // e.g., "Player", "Ally"
    public float detectionRange = 20;
    public LayerMask targetsLayerMask; // So we only detect what we need to
    [SerializeField] protected float targetSwitchThreshold = 100f;
    private bool passedThreshold = false;
    private Collider[] detectedColliders;      // Array to store detected colliders
    private int maxDetectedObjects = 10; // Max number of objects the enemy can detect at once
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
        agent.stoppingDistance = stoppingDistance;
        detectedColliders = new Collider[maxDetectedObjects]; // Pre-allocate the array for detected objects
        DetectTargets();
        CheckDamageThreshold(health.maxHP - health.CurrentHP);
        SetRandomState();
    }
    protected virtual void Update()
    {
        UpdateStateTimer();

        if (!isAttacking)
        {
            UpdateCurrentState();
        }
    }
    protected virtual void UpdateStateTimer()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            SetRandomState();
        }
    }
    protected virtual void SetRandomState()
    {
        List<StateWeight> stateWeights = CalculateStateWeights();

        // Normalize weights
        float totalWeight = stateWeights.Sum(sw => sw.weight);
        if (totalWeight > 0)
        {
            float randomValue = Random.value * totalWeight;
            float currentSum = 0;

            foreach (var stateWeight in stateWeights)
            {
                currentSum += stateWeight.weight;
                //Debug.Log($"{stateWeight.state} State - Weight: {stateWeight.weight}");
                if (randomValue <= currentSum)
                {
                    Debug.Log($"Entering {stateWeight.state} State - Weight: {stateWeight.weight}");
                    SetState(stateWeight.state);
                    return;
                }
            }
        }

        // Fallback to idle if something goes wrong
        SetState(EnemyState.Idle);
    }
    protected virtual List<StateWeight> CalculateStateWeights()
    {
        List<StateWeight> weights = new List<StateWeight>();
        float distanceToTarget = currentTarget ? Vector3.Distance(transform.position, currentTarget.position) : float.MaxValue;
        float recentDamageSum = CalculateRecentDamage();
        // Chase Priority
        float chaseWeight = CalculateChaseWeight(distanceToTarget);
        weights.Add(new StateWeight(EnemyState.Chase, chaseWeight));
        // Attack Priority
        float attackWeight = CalculateAttackWeight(distanceToTarget);
        weights.Add(new StateWeight(EnemyState.Attack, attackWeight));
        // Defensive Priorities
        float retreatWeight = CalculateRetreatWeight(recentDamageSum, distanceToTarget);
        weights.Add(new StateWeight(EnemyState.Retreat, retreatWeight));
        float strafeWeight = CalculateStrafeWeight(recentDamageSum, distanceToTarget);
        weights.Add(new StateWeight(EnemyState.Strafe, strafeWeight));

        // Idle is lowest priority
        weights.Add(new StateWeight(EnemyState.Idle, 0.01f));

        return weights;
    }
    protected virtual float CalculateChaseWeight(float distanceToTarget)
    {
        if (distanceToTarget > chasePriorityDistance)
            return 3f;
        else if (distanceToTarget > stoppingDistance)
            return 1.5f;
        return 0.1f;
    }
    protected virtual float CalculateAttackWeight(float distanceToTarget)
    {
        Attack bestAttack = ChooseBestAttack(distanceToTarget);
        if (bestAttack != null)
            return 2.5f;
        return 0.5f;
    }
    protected virtual float CalculateRetreatWeight(float recentDamage, float distanceToTarget)
    {
        float weight = 0.1f;
        if (recentDamage <= 0)
            return 0;

        if (recentDamage > damagePriorityThreshold && distanceToTarget < stoppingDistance * 1.5f)
            weight += 2f;

        return weight;
    }
    protected virtual float CalculateStrafeWeight(float recentDamage, float distanceToTarget)
    {
        float weight = 0.2f; // Base weight for strafing

        if (recentDamage > damagePriorityThreshold * 0.5f)
            weight += 1.5f;

        if (distanceToTarget <= stoppingDistance * 1.5f)
            weight -= 0.3f;

        return weight;
    }
    protected float CalculateRecentDamage()
    {
        float currentTime = Time.time;

        // Remove old damage instances
        while (recentDamage.Count > 0 && currentTime - recentDamage.Peek().time > damageTrackingDuration)
        {
            recentDamage.Dequeue();
        }
        return recentDamage.Sum(d => d.amount);
    }
    protected virtual void SetState(EnemyState newState)
    {
        currentState = newState;

        // Set appropriate timer for the new state
        switch (currentState)
        {
            case EnemyState.Idle:
                stateTimer = Random.Range(2f, 4f);
                break;
            case EnemyState.Strafe:
                stateTimer = Random.Range(2f, 4f);
                CalculateStrafePosition();
                break;
            case EnemyState.Retreat:
                stateTimer = Random.Range(1f, 3f);
                break;
            case EnemyState.Attack:
                //intervalCooldown = Random.Range(minAttackInterval, maxAttackInterval);
                stateTimer = Random.Range(5f, 8f);
                break;
            case EnemyState.Chase:
                DetectTargets();
                stateTimer = Random.Range(1f, 4f);
                break;
            default:
                stateTimer = Random.Range(2f, 4f);
                break;
        }

        if (animator != null)
            animator.SetInteger("State", (int)currentState);
    }
    protected virtual void UpdateCurrentState()
    {
        // for when current target is destroyed find a new target
        if (currentTarget == null)
            DetectTargets();
        // alot of behavior relies on distance to current target
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                // Debug.Log("Idling");
                UpdateIdleState();
                break;
            case EnemyState.Chase:
                // Debug.Log("Chasing");
                UpdateChaseState(distanceToTarget);
                break;
            case EnemyState.Attack:
                //Debug.Log("Attacking");
                UpdateAttackState(distanceToTarget);
                break;
            case EnemyState.Retreat:
                // Debug.Log("Retreating");
                UpdateRetreatState();
                break;
            case EnemyState.Strafe:
                // Debug.Log("Strafing");
                UpdateStrafeState();
                break;
        }
    }
    protected virtual void UpdateIdleState()
    {
        agent.isStopped = true;
        // Possibly look at target or play idle animation
        if (currentTarget != null)
        {
            Vector3 lookDirection = (currentTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
        }
    }
    protected virtual void UpdateChaseState(float distanceToTarget)
    {
        if (passedThreshold)
            DetectTargets();

        if (distanceToTarget > stoppingDistance)
        {
            // agent stopping distance changes based on different behaviors
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
            if (!targetingStructure)
            {
                Vector3 pos = currentTarget.GetComponent<Collider>().ClosestPoint(transform.position);
                pos.y = transform.position.y;
                agent.SetDestination(pos);
            }
            else
            {
                agent.SetDestination(currentTarget.position);
            }
           
        }
        else
        {
            currentState = EnemyState.Attack;
        }
    }
    protected virtual void UpdateAttackState(float distanceToTarget)
    {
        /*if (intervalCooldown <= 0)
        {*/
            // so that it doesnt go through all attacks, added a random chance to not attack and do something else instead
            int index = Random.Range(0, 100);
            //Debug.Log(distanceToTarget);
            Attack bestAttack = ChooseBestAttack(distanceToTarget);
            if (bestAttack != null && index < 70)
            {
                // Get the direction to the target
                Vector3 direction = (currentTarget.position - transform.position).normalized;
                // Calculate the rotation needed to face the target
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                // Smoothly rotate towards the target
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5 * Time.deltaTime);
               
                Vector3 lookDirection = (currentTarget.position - transform.position).normalized;
                lookDirection.y = 0;
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
                //Debug.Log("Attacking With: " + bestAttack.name);
                StartCoroutine(bestAttack.ExecuteAttack(this, currentTarget));
                return;
            }
            else
            {
                SetRandomState(); // Choose new state if we can't attack
            }
        

        //intervalCooldown -= Time.deltaTime;
    }
    protected virtual void UpdateRetreatState()
    {
        if (currentTarget != null)
        {
            agent.stoppingDistance = stoppingDistance / 2;

            Vector3 retreatDirection = transform.position - currentTarget.position;
            Vector3 retreatPosition = transform.position + retreatDirection.normalized * retreatDistance;

            if (NavMesh.SamplePosition(retreatPosition, out NavMeshHit hit, retreatDistance, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
    }
    protected virtual void CalculateStrafePosition()
    {
        if (currentTarget != null)
        {
            strafeDirectionRight = !strafeDirectionRight; // Alternate strafe direction
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            Vector3 perpendicularDirection = strafeDirectionRight ?
                Vector3.Cross(directionToTarget, Vector3.up) :
                Vector3.Cross(Vector3.up, directionToTarget);

            Vector3 potentialStrafePosition = transform.position + perpendicularDirection * strafeRadius;

            if (NavMesh.SamplePosition(potentialStrafePosition, out NavMeshHit hit, strafeRadius, NavMesh.AllAreas))
            {
                strafeTarget = hit.position;
            }
        }
    }
    protected virtual void UpdateStrafeState()
    {
        if (currentTarget != null)
        {
            agent.stoppingDistance = stoppingDistance / 2;
            // Move to strafe position
            agent.isStopped = false;
            agent.SetDestination(strafeTarget);

            // Look at target while strafing
            Vector3 lookDirection = (currentTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

            // If we're close to the strafe target, calculate a new one
            if (Vector3.Distance(transform.position, strafeTarget) < 1f)
            {
                CalculateStrafePosition();
            }
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
            agent.isStopped = false;
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
                Debug.Log("Cannot Use Attack: " + attack.name);
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
    void DetectTargets()
    {
        int detectedCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, detectedColliders, targetsLayerMask);

        // Sort by priority target based on tag, and then by closest distance
        currentTarget = detectedColliders
           .Where(c => c != null && priorityTags.Contains(c.tag))    // Filter by priority tags
           .OrderBy(c => GetPriorityIndex(c.tag))                    // Prioritize by tag order
           .ThenBy(c => Vector3.Distance(transform.position, c.transform.position)) // If same tag, choose closest
           .Select(c => c.transform)
           .FirstOrDefault();
        targetingStructure = true;

        if (currentTarget == null && train != null)
        {
            targetingStructure = false;
            if (train.Payload != null)
                currentTarget = train.Payload.transform;
            else
                currentTarget = train.GetDamagePoint();
        }
    }
    // Get the priority index of the tag, lower numbers mean higher priority
    int GetPriorityIndex(string tag)
    {
        for (int i = 0; i < priorityTags.Length; i++)
        {
            if (priorityTags[i] == tag)
            {
                return i; // Priority is determined by index in the array
            }
        }
        return priorityTags.Length; // If the tag isn't a priority, return a lower priority index
    }
    public virtual void Die()
    {
        OnEnemyDeath?.Invoke();
        Destroy(gameObject);
    }
    public void CheckDamageThreshold(float damageTaken)
    {
        passedThreshold = damageTaken >= targetSwitchThreshold;
    }
    public void AssignDefaultTarget(TrainHandler train, Transform target)
    {
        this.train = train;
        currentTarget = target;
    }
}
