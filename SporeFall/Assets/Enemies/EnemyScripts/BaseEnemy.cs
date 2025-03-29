using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Retreat,
    Strafe
}

// Abstract base class for all Enemies
public abstract class BaseEnemy : MonoBehaviour
{
    // Public events
    public delegate void EnemyDeath(BaseEnemy enemy);
    public event EnemyDeath OnEnemyDeath;

    [SerializeField] protected Attack[] attacks;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Damageable health;
    protected AudioSource audioSource;
    public NavMeshAgent agent;
    public Transform firePoint;

    [Header("Strafe Behavior")]
    [SerializeField] protected float strafeRadius = 10f;
    [SerializeField] protected float retreatDistance = 15f;
    [SerializeField] protected float strafeSpeed = 5f;
    protected EnemyState currentState = EnemyState.Idle;
    private float stateTimer;
    private bool strafeDirectionRight = true;
    [HideInInspector]
    public Vector3 strafeTarget;

    [Header("State Selection")]
    [SerializeField] protected float chasePriorityDistance = 25f;
    [SerializeField] protected float damagePriorityThreshold = 20f;
    [SerializeField] protected float damageTrackingDuration = 3f;
    private bool targetingStructure = false;

    // Use a simple array instead of Queue for better performance
    private const int MAX_DAMAGE_HISTORY = 10;
    private DamageInstance[] damageHistory;
    private int damageHistoryCount = 0;
    private int damageHistoryIndex = 0;

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

    // Reuse this list to avoid allocations
    private List<StateWeight> stateWeightsList = new List<StateWeight>(5);
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
    [SerializeField] protected float aggressionFactor = 0.6f;
    protected bool isAttacking;

    // Cached best attack to avoid recalculation
    protected Attack cachedBestAttack;
    protected float cachedAttackDistance;
    protected float cachedAttackTime;
    protected float attackCacheTime = 0.5f;

    [Header("Targeting")]
    public TrainHandler train;
    private Transform trainWall;
    public Transform currentTarget;
    public string[] priorityTags;
    public float detectionRange = 20;
    public LayerMask targetsLayerMask;
    [SerializeField] protected float targetSwitchThreshold = 100f;
    private bool passedThreshold = false;

    // For target detection optimization
    private Collider[] detectedColliders;
    private int maxDetectedObjects = 25;
    private float targetDetectionInterval = 0.5f;
    private float targetDetectionTimer = 0;
    private int lastDetectedCount = 0;

    // Position caching to reduce collider calculations
    private Vector3 cachedTargetPosition;
    private float targetPositionCacheTime = 0.2f;
    private float targetPositionCacheTimer = 0;

    public Animator Animator => animator;
    public AudioSource AudioSource => audioSource;

    private bool isInitialized = false;
    private bool isRising = false;

    [Header("Animations")]
    [SerializeField] private float risingAnimationLength = 2;

    // State machine references
    private Dictionary<EnemyState, IEnemyState> states;
    private IEnemyState currentStateObj;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        detectedColliders = new Collider[maxDetectedObjects];
        damageHistory = new DamageInstance[MAX_DAMAGE_HISTORY];

        // Initialize state machine
        InitializeStateMachine();
    }
    protected virtual void InitializeStateMachine()
    {
        states = new Dictionary<EnemyState, IEnemyState>
        {
            { EnemyState.Idle, new IdleState(this) },
            { EnemyState.Chase, new ChaseState(this) },
            { EnemyState.Attack, new AttackState(this) },
            { EnemyState.Retreat, new RetreatState(this) },
            { EnemyState.Strafe, new StrafeState(this) }
        };

        currentStateObj = states[EnemyState.Idle];
    }


    protected virtual void OnEnable()
    {
        if (!isInitialized)
        {
            Initialize();
            isInitialized = true;
        }

        // Use a simple repeated check instead of coroutine for better performance
        targetDetectionTimer = 0;
        ResetState();
    }
    protected virtual void Initialize()
    {
        agent.stoppingDistance = stoppingDistance;
    }
    protected virtual void ResetState()
    {
        // Reset all state when object is reused from pool
        foreach (var att in attacks)
        {
            att.ResetCooldown();
        }

        // Reset variables
        currentState = EnemyState.Idle;
        currentStateObj = states[EnemyState.Idle];
        isAttacking = false;
        passedThreshold = false;
        targetingStructure = false;
        strafeDirectionRight = true;
        stateTimer = 0f;

        // Clear the damage history
        damageHistoryCount = 0;
        damageHistoryIndex = 0;

        // Reset health
        if (health != null)
        {
            health.ResetHealth();
        }

        // Reset NavMeshAgent
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.ResetPath();
            agent.isStopped = false;
            agent.velocity = Vector3.zero;
        }

        // Start behavior if not rising
        if (!isRising)
        {
            DetectTargets();
            SetRandomState();
        }
    }

    protected virtual void Update()
    {
        // Handle target detection on a timer instead of every frame
        targetDetectionTimer -= Time.deltaTime;
        if (targetDetectionTimer <= 0)
        {
            if (currentTarget == null || !currentTarget.gameObject.activeSelf)
            {
                DetectTargets();
            }
            targetDetectionTimer = targetDetectionInterval;
        }

        UpdateStateTimer();

        if (!isAttacking)
        {
            // Use the state pattern
            currentStateObj.UpdateState();
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
        // Clear and reuse the list
        stateWeightsList.Clear();

        float distanceToTarget = GetDistanceToTarget();
        float recentDamageSum = CalculateRecentDamage();

        // Add state weights to our reused list
        stateWeightsList.Add(new StateWeight(EnemyState.Chase, CalculateChaseWeight(distanceToTarget)));
        stateWeightsList.Add(new StateWeight(EnemyState.Attack, CalculateAttackWeight(distanceToTarget)));
        stateWeightsList.Add(new StateWeight(EnemyState.Retreat, CalculateRetreatWeight(recentDamageSum, distanceToTarget)));
        stateWeightsList.Add(new StateWeight(EnemyState.Strafe, CalculateStrafeWeight(recentDamageSum, distanceToTarget)));
        stateWeightsList.Add(new StateWeight(EnemyState.Idle, 0.01f));

        // Calculate total weight
        float totalWeight = 0;
        for (int i = 0; i < stateWeightsList.Count; i++)
        {
            totalWeight += stateWeightsList[i].weight;
        }

        if (totalWeight > 0)
        {
            float randomValue = Random.value * totalWeight;
            float currentSum = 0;

            for (int i = 0; i < stateWeightsList.Count; i++)
            {
                currentSum += stateWeightsList[i].weight;
                if (randomValue <= currentSum)
                {
                    SetState(stateWeightsList[i].state);
                    return;
                }
            }
        }

        // Fallback to idle if something goes wrong
        SetState(EnemyState.Idle);
    }
   /* protected virtual List<StateWeight> CalculateStateWeights()
    {
        List<StateWeight> weights = new();
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
    }*/
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
        Attack bestAttack = GetBestAttack(distanceToTarget);
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
        float weight = 0.1f; // Base weight for strafing

        if (recentDamage > damagePriorityThreshold * 0.5f)
            weight += 1.5f;
        else if (recentDamage <= 0)
            return 0;

        return weight;
    }
    public virtual void CalculateStrafePosition()
    {
        if (currentTarget != null)
        {
            strafeDirectionRight = !strafeDirectionRight;
            Vector3 directionToTarget = (GetTargetPosition() - transform.position).normalized;
            Vector3 perpendicularDirection = strafeDirectionRight ?
                Vector3.Cross(directionToTarget, Vector3.up) :
                Vector3.Cross(Vector3.up, directionToTarget);

            Vector3 potentialStrafePosition = transform.position + perpendicularDirection * strafeRadius;

            // Simplify NavMesh sampling with a raycast first
            if (Physics.Raycast(potentialStrafePosition + Vector3.up * 5, Vector3.down, out RaycastHit hit, 10f))
            {
                potentialStrafePosition = hit.point;
            }

            if (NavMesh.SamplePosition(potentialStrafePosition, out NavMeshHit navHit, strafeRadius, NavMesh.AllAreas))
            {
                strafeTarget = navHit.position;
            }
        }
    }
    public void AddDamageInstance(float amount)
    {
        float currentTime = Time.time;

        if (damageHistoryCount < MAX_DAMAGE_HISTORY)
        {
            damageHistory[damageHistoryCount] = new DamageInstance(amount, currentTime);
            damageHistoryCount++;
        }
        else
        {
            // Replace oldest entry in circular buffer
            damageHistory[damageHistoryIndex] = new DamageInstance(amount, currentTime);
            damageHistoryIndex = (damageHistoryIndex + 1) % MAX_DAMAGE_HISTORY;
        }
    }
    protected float CalculateRecentDamage()
    {
        float currentTime = Time.time;
        float totalDamage = 0;

        for (int i = 0; i < damageHistoryCount; i++)
        {
            if (currentTime - damageHistory[i].time <= damageTrackingDuration)
            {
                totalDamage += damageHistory[i].amount;
            }
        }

        return totalDamage;
    }
    protected virtual void SetState(EnemyState newState)
    {
        currentState = newState;
        currentStateObj = states[newState];
        currentStateObj.EnterState();

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
                stateTimer = Random.Range(5f, 8f);
                break;
            case EnemyState.Chase:
                stateTimer = Random.Range(1f, 4f);
                break;
            default:
                stateTimer = Random.Range(2f, 4f);
                break;
        }

        if (animator != null)
            animator.SetInteger("State", (int)currentState);
    }
/*    protected virtual void UpdateCurrentState()
    {
        // for when current target is destroyed find a new target
        if (currentTarget == null)
        {
            DetectTargets();
        }else if(currentTarget.gameObject.activeSelf == false)
        {
            DetectTargets();
        }
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

        Vector3 targetPosition;

        if (!targetingStructure)
        {
            // Get closest point on the target's collider
            targetPosition = currentTarget.GetComponent<Collider>().ClosestPoint(transform.position);
            targetPosition.y = transform.position.y;

            // Update distance calculation using the closest point
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        }
        else
        {
            // Structures
            targetPosition = currentTarget.position;
        }

        // Check if we're outside the stopping distance
        if (distanceToTarget > stoppingDistance)
        {
            // Set up agent for movement
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
            agent.SetDestination(targetPosition);
        }
        else
        {
            // We're within attack range, transition to attack state
            agent.isStopped = true;
            SetState(EnemyState.Attack);
        }
    }
    protected virtual void UpdateAttackState(float distanceToTarget)
    {
        Vector3 targetPosition;

        if (!targetingStructure)
        {
            // Get closest point on the target's collider for consistent distance calculation
            targetPosition = currentTarget.GetComponent<Collider>().ClosestPoint(transform.position);
            targetPosition.y = transform.position.y;
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        }
        else
        {
            targetPosition = currentTarget.position;
        }

        Attack bestAttack = ChooseBestAttack(distanceToTarget);
        if (bestAttack != null && currentTarget.gameObject.activeSelf)
        {
            // Calculate direction to the actual target position for rotation
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // Keep the enemy level

            // Smoothly rotate towards the target
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5 * Time.deltaTime);

            StartCoroutine(bestAttack.ExecuteAttack(this, currentTarget));
            return;
        }
        else
        {
            //Debug.Log("Cannot Attack");
            SetRandomState(); // Choose new state if we can't attack
        }
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
    }*/
    public void SetIsAttacking(bool attacking)
    {
        isAttacking = attacking;
        if (!agent.isActiveAndEnabled)
            return;

        agent.isStopped = attacking;
    }
    public virtual Attack GetBestAttack(float distanceToTarget)
    {
        float currentTime = Time.time;

        // Use cached result if it's recent and distance hasn't changed much
        if (cachedBestAttack != null &&
            currentTime - cachedAttackTime < attackCacheTime &&
            Mathf.Abs(cachedAttackDistance - distanceToTarget) < 2f)
        {
            return cachedBestAttack;
        }

        // Recalculate best attack
        Attack bestAttack = null;
        float bestPriority = float.MinValue;

        for (int i = 0; i < attacks.Length; i++)
        {
            if (attacks[i].CanUse(distanceToTarget))
            {
                float priority = EvaluateAttackPriority(attacks[i], distanceToTarget);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestAttack = attacks[i];
                }
            }
        }

        // Cache the result
        cachedBestAttack = bestAttack;
        cachedAttackDistance = distanceToTarget;
        cachedAttackTime = currentTime;

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
        WaitForSeconds waitTickTime = new WaitForSeconds(tickRate);

        while (elapsedTime < duration)
        {
            target.TakeDamage(damagePerTick);
            yield return waitTickTime;
            elapsedTime += tickRate;
        }
    }
    void DetectTargets()
    {
        int detectedCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, detectedColliders, targetsLayerMask);
        lastDetectedCount = detectedCount;
        // Early exit if nothing detected
        if (detectedCount == 0)
        {
            if (train != null)
            {
                targetingStructure = false;
                if (train.Payload != null)
                    currentTarget = train.Payload.transform;
                else
                    currentTarget = trainWall;
            }
            return;
        }

        // Find best target
        Transform bestTarget = null;
        int bestPriority = int.MaxValue;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < detectedCount; i++)
        {
            var collider = detectedColliders[i];
            if (collider == null || !collider.gameObject.activeSelf)
                continue;

            string tag = collider.tag;
            if (!System.Array.Exists(priorityTags, t => t == tag))
                continue;

            if (!IsTargetAccessible(collider.transform))
                continue;

            int priority = GetPriorityIndex(tag);
            if (priority < bestPriority ||
                (priority == bestPriority &&
                 Vector3.Distance(transform.position, collider.transform.position) < bestDistance))
            {
                bestTarget = collider.transform;
                bestPriority = priority;
                bestDistance = Vector3.Distance(transform.position, collider.transform.position);
            }
        }

        targetingStructure = bestTarget == null || bestTarget.CompareTag("Structure");

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            targetPositionCacheTimer = 0; // Force update of cached position
        }
        else if (train != null)
        {
            targetingStructure = false;
            if (train.Payload != null)
                currentTarget = train.Payload.transform;
            else
                currentTarget = trainWall;
        }
    }
    public Vector3 GetTargetPosition()
    {
        if (currentTarget == null)
            return transform.position;

        targetPositionCacheTimer -= Time.deltaTime;

        if (targetPositionCacheTimer <= 0)
        {
            if (!targetingStructure && currentTarget.TryGetComponent<Collider>(out var collider))
            {
                cachedTargetPosition = collider.ClosestPoint(transform.position);
                cachedTargetPosition.y = transform.position.y; // Keep level with enemy
            }
            else
            {
                cachedTargetPosition = currentTarget.position;
            }

            targetPositionCacheTimer = targetPositionCacheTime;
        }

        return cachedTargetPosition;
    }
    // Get distance to target using cached position when possible
    public float GetDistanceToTarget()
    {
        if (currentTarget == null)
            return float.MaxValue;

        return Vector3.Distance(transform.position, GetTargetPosition());
    }

    // Optimization: check accessibility less frequently
    private bool IsTargetAccessible(Transform target)
    {
        if (target == null) return false;

        // Simple distance check first (optimization)
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > detectionRange * 1.5f)
            return false;

        // Use a simple raycast first as it's faster than NavMesh operations
        if (Physics.Linecast(transform.position, target.position, out RaycastHit hitInfo, LayerMask.GetMask("Obstacles")))
        {
            // If there's an obstacle, do the more expensive NavMesh check
            if (NavMesh.SamplePosition(target.position, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(navHit.position, path))
                {
                    return path.status == NavMeshPathStatus.PathComplete;
                }
            }
            return false;
        }

        // If no obstacles detected by raycast, assume it's accessible
        return true;
    }

   /* private IEnumerator PeriodicTargetDetection()
    {
        WaitForSeconds waitTime = new(4f); // Adjust interval as needed

        while (gameObject.activeInHierarchy)
        {
            DetectTargets();
            yield return waitTime;
        }
    }*/
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
        OnEnemyDeath?.Invoke(this); // Pass 'this' to the event
        gameObject.SetActive(false); // Deactivate instead of destroy
    }
    public void AssignDefaultTarget(TrainHandler train, Transform target)
    {
        this.train = train;
        currentTarget = target;

        ChooseTrainWall();
    }
    private void ChooseTrainWall()
    {
        if (train != null)
        {
            trainWall = train.GetDamagePoint();
        }
    }
    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        currentTarget = null;
        train = null;
        SetIsAttacking(false);

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }
    public void CheckDamageThreshold(float damageTaken)
    {
        passedThreshold = damageTaken >= targetSwitchThreshold;
        // Add to damage history
        AddDamageInstance(damageTaken);
    }
    public void TriggerRiseAnimation()
    {
        animator.SetTrigger("Rise");
        StartCoroutine(RisingFromGround());
    } 
    private IEnumerator RisingFromGround()
    {
        SetState(EnemyState.Idle);
        agent.isStopped = true;
        float temp = agent.speed;
        agent.speed = 0;
        isRising = true;
        //Debug.Log("Rising From Ground");
        yield return new WaitForSeconds(risingAnimationLength);
        //Debug.Log("Done Rising");
        agent.isStopped = false;
        agent.speed = temp;
        isRising = false;

        DetectTargets();
        SetRandomState();
    }
}
