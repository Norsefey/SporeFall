using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public delegate void EnemyDeath(BaseEnemy enemy); // Modified to pass the enemy instance
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
    private Vector3 strafeTarget;
    private bool strafeDirectionRight = true;
    [Header("State Selection")]
    [SerializeField] protected float chasePriorityDistance = 25f; // Distance above which chase becomes high priority
    [SerializeField] protected float damagePriorityThreshold = 20f; // Amount of damage that triggers defensive priorities
    [SerializeField] protected float damageTrackingDuration = 3f; // How long to track damage for
    private bool targetingStructure = false;
    public Queue<DamageInstance> recentDamage = new();
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
/*    [SerializeField] protected float minAttackInterval = 5;
    [SerializeField] protected float maxAttackInterval = 8;*/
    [SerializeField] protected float aggressionFactor = 0.6f; // Chance to choose aggressive actions
    protected bool isAttacking;

    [Header("Targeting")]
    public TrainHandler train; // if nothing is in range will move to Payload or train
    private Transform trainWall;
    public Transform currentTarget;
    // Array to hold multiple valid target tags
    public string[] priorityTags; // e.g., "Player", "Ally"
    public float detectionRange = 20;
    public LayerMask targetsLayerMask; // So we only detect what we need to
    [SerializeField] protected float targetSwitchThreshold = 100f;
    private bool passedThreshold = false;
    private Collider[] detectedColliders;      // Array to store detected colliders
    private int maxDetectedObjects = 25; // Max number of objects the enemy can detect at once
    public Animator Animator => animator;
    public AudioSource AudioSource => audioSource;

    private bool isInitialized = false;
    private bool isRising = false;
    [Header("Animations")]
    [SerializeField] private float risingAnimationLength = 2;

    protected virtual void Awake()
    {
        // Get component references once
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        detectedColliders = new Collider[maxDetectedObjects];
    }

    protected virtual void OnEnable()
    {
        if (!isInitialized)
        {
            Initialize();
            isInitialized = true;
        }
        StartCoroutine(PeriodicTargetDetection());
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
        isAttacking = false;
        passedThreshold = false;
        targetingStructure = false;
        strafeDirectionRight = true;
        stateTimer = 0f;

        // Reset queues and collections
        recentDamage.Clear();

        // Reset health
        if (health != null)
        {
            health.ResetHealth();
        }

        // Reset NavMeshAgent
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = false;
            agent.velocity = Vector3.zero;
        }

       /* // Reset animation
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }*/

        // do not want movement while rising from from ground
        if (!isRising)
        {
            // Start behavior
            DetectTargets();
            SetRandomState();
        }
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
                    //Debug.Log($"Entering {stateWeight.state} State - Weight: {stateWeight.weight}");
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
        float weight = 0.01f; // Base weight for strafing

        if (recentDamage > damagePriorityThreshold * 0.5f)
            weight += 1.5f;
        else if (recentDamage <= 0)
            return 0;

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
    }
    public void SetIsAttacking(bool attacking)
    {
        isAttacking = attacking;
        if (!agent.isActiveAndEnabled)
            return;
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

        currentTarget = detectedColliders
        .Where(c => c != null && c.gameObject.activeSelf && priorityTags.Contains(c.tag))    // Filter by priority tags
        .Where(c => IsTargetAccessible(c.transform))              // Filter by NavMesh accessibility
        .OrderBy(c => GetPriorityIndex(c.tag))                   // Prioritize by tag order
        .ThenBy(c => Vector3.Distance(transform.position, c.transform.GetComponent<Collider>().ClosestPoint(transform.position))) // If same tag, choose closest
        .Select(c => c.transform)
        .FirstOrDefault();
        targetingStructure = true;

        if (train != null && (currentTarget == null || currentTarget.CompareTag("Train")))
        {
            targetingStructure = false;
            if (train.Payload != null)
                currentTarget = train.Payload.transform;
            else
                currentTarget = trainWall;
        }
    }
    private IEnumerator PeriodicTargetDetection()
    {
        WaitForSeconds waitTime = new(4f); // Adjust interval as needed

        while (gameObject.activeInHierarchy)
        {
            DetectTargets();
            yield return waitTime;
        }
    }
    // Add this helper method to check if a target position is accessible via NavMesh
    private bool IsTargetAccessible(Transform target)
    {
        if (target == null) return false;

        // Get the nearest valid position on NavMesh to the target
        Vector3 targetPosition = target.position;

        // First check if the target is directly on a NavMesh
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            // Calculate path to target
            NavMeshPath path = new();
            if (agent.CalculatePath(hit.position, path))
            {
                // Check if the path is complete and valid
                return path.status == NavMeshPathStatus.PathComplete;
            }
        }

        // If target isn't directly on NavMesh, try to find the nearest valid position
        if (NavMesh.SamplePosition(targetPosition, out hit, 2.0f, NavMesh.AllAreas))
        {
            NavMeshPath path = new();
            if (agent.CalculatePath(hit.position, path))
            {
                return path.status == NavMeshPathStatus.PathComplete;
            }
        }

        return false;
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
    // Optional: Add method to clean up any persistent effects or coroutines when returned to pool
    protected virtual void OnDisable()
    {
        StopAllCoroutines();

        // Clean up any references
        currentTarget = null;
        train = null;

        // Reset any ongoing effects or states
        SetIsAttacking(false);

        if (agent.isActiveAndEnabled)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }
    public void CheckDamageThreshold(float damageTaken)
    {
        passedThreshold = damageTaken >= targetSwitchThreshold;
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
        Debug.Log("Rising From Ground");
        yield return new WaitForSeconds(risingAnimationLength);
        Debug.Log("Done Rising");

        agent.isStopped = false;
        agent.speed = temp;

        isRising = false;
        DetectTargets();
        SetRandomState();
    }
}
