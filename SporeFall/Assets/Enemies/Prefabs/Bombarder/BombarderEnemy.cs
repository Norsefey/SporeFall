using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BombarderEnemy : BaseEnemy
{
    [Header("Stationary Range Settings")]
    [SerializeField] private float optimalRange = 30f;            // The ideal range this enemy prefers to attack from
    [SerializeField] private float rotationSpeed = 5f;            // How fast the enemy can rotate to face targets
    [SerializeField] private float attackAngleThreshold = 15f;    // How accurate the aim needs to be to attack (in degrees)
    [SerializeField] private float detectionRangeMultiplier = 2f; // Multiplier for detection range compared to attack range
    [SerializeField] private Transform turretPivot;               // Optional pivot point for rotating the weapon/turret
    [SerializeField] private bool canRotateBody = false;          // Whether the entire body can rotate or just the turret
    [SerializeField] private float alertRadius = 50f;             // How far the enemy can hear combat and become alerted

    [Header("Repositioning Behavior")]
    [SerializeField] private float inactivityTimeBeforeMove = 30f;    // Time without detecting targets before repositioning
    [SerializeField] private float repositionRadius = 15f;            // Maximum distance to reposition
    [SerializeField] private float sinkingDuration = 2f;              // How long it takes to sink
    [SerializeField] private float movementSpeed = 10f;               // Movement speed when underground
    [SerializeField] private LayerMask repositionLayerMask;           // Layers to avoid when repositioning
    [SerializeField] private float minDistanceFromObstacles = 3f;     // Min distance from obstacles when repositioning
    [SerializeField] private GameObject sinkVFXPrefab;                // VFX for sinking

    private bool isAlerted = false;
    private Vector3 originalPosition;
    private float lastDetectionTime;
    private bool isRepositioning = false;
    private Vector3 targetRepositionPoint;
    private bool isSinking = false;
    private bool isUnderground = false;
    private float originalYPosition;

    protected override void Awake()
    {
        base.Awake();

        // Store the original position and Y height when spawned
        originalPosition = transform.position;
        originalYPosition = transform.position.y;

        // Increase detection range based on optimal range
        detectionRange = optimalRange * detectionRangeMultiplier;

        // Set initial last detection time
        lastDetectionTime = Time.time;
    }
    protected override void Initialize()
    {
        base.Initialize();

        // Disable NavMeshAgent movement normally
        if (agent != null)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
    }
    protected override void Update()
    {
        base.Update();

        // Check if we need to reposition
        if (!isRepositioning && !isSinking && !isUnderground && !isAttacking &&
            (Time.time - lastDetectionTime) > inactivityTimeBeforeMove)
        {
            StartCoroutine(RepositionCoroutine());
        }
    }
    protected override void UpdateIdleState()
    {
        // Don't perform idle behaviors while repositioning
        if (isRepositioning || isSinking || isUnderground)
            return;

        // In idle state, slowly scan the area or return to default rotation
        if (!isAlerted)
        {
            // Implement scanning behavior if desired
            // For example, slowly rotate back and forth
            float rotationAngle = Mathf.Sin(Time.time * 0.5f) * 45f;
            Quaternion targetRotation = Quaternion.Euler(0, rotationAngle, 0) * Quaternion.Euler(0, transform.eulerAngles.y, 0);

            if (canRotateBody)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed * 0.5f);
            }
            else if (turretPivot != null)
            {
                turretPivot.rotation = Quaternion.Slerp(turretPivot.rotation, targetRotation, Time.deltaTime * rotationSpeed * 0.5f);
            }
        }
        else
        {
            // If alerted but no target, continue facing last known direction
            // or implement a "searching" behavior
        }

        // Listen for combat nearby
        CheckForCombatSounds();
    }
    protected override void UpdateChaseState(float distanceToTarget)
    {
        // Don't perform chase behaviors while repositioning
        if (isRepositioning || isSinking || isUnderground)
            return;

        // Override chase state to not move but to rotate toward target
        if (currentTarget != null)
        {
            // Update last detection time since we have a target
            lastDetectionTime = Time.time;

            // Calculate direction to target
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            directionToTarget.y = 0; // Keep level

            // Rotate to face target
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            if (canRotateBody)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else if (turretPivot != null)
            {
                turretPivot.rotation = Quaternion.Slerp(turretPivot.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            // If we're facing the target closely enough, transition to attack state
            float angle = Vector3.Angle(turretPivot != null ? turretPivot.forward : transform.forward, directionToTarget);
            if (angle < attackAngleThreshold)
            {
                SetState(EnemyState.Attack);
            }
        }
        else
        {
            // No target, go back to idle
            SetState(EnemyState.Idle);
        }
    }
    protected override void UpdateAttackState(float distanceToTarget)
    {
        // Don't perform attack behaviors while repositioning
        if (isRepositioning || isSinking || isUnderground)
            return;

        if (currentTarget == null)
        {
            SetState(EnemyState.Idle);
            return;
        }

        // Update last detection time since we have a target
        lastDetectionTime = Time.time;

        // Always aim at target while in attack state
        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
        directionToTarget.y = 0; // Keep level

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        if (canRotateBody)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else if (turretPivot != null)
        {
            turretPivot.rotation = Quaternion.Slerp(turretPivot.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Check if we're facing target before attacking
        float angle = Vector3.Angle(turretPivot != null ? turretPivot.forward : transform.forward, directionToTarget);
        if (angle < attackAngleThreshold)
        {
            // Select and execute attack
            Attack bestAttack = ChooseBestAttack(distanceToTarget);
            if (bestAttack != null && currentTarget.gameObject.activeSelf)
            {
                StartCoroutine(bestAttack.ExecuteAttack(this, currentTarget));
            }
        }

        
    }
    protected override void UpdateRetreatState()
    {
        // Stationary enemies don't retreat - they just focus on target rotation
        SetState(EnemyState.Attack);
    }

    protected override void UpdateStrafeState()
    {
        // Stationary enemies don't strafe - they just focus on target rotation
        SetState(EnemyState.Attack);
    }
    protected override List<StateWeight> CalculateStateWeights()
    {
        // Don't calculate states while repositioning
        if (isRepositioning || isSinking || isUnderground)
        {
            List<StateWeight> idleOnly = new List<StateWeight>();
            idleOnly.Add(new StateWeight(EnemyState.Idle, 1.0f));
            return idleOnly;
        }

        List<StateWeight> weights = new List<StateWeight>();

        float distanceToTarget = currentTarget ? Vector3.Distance(transform.position, currentTarget.position) : float.MaxValue;

        // For stationary enemies, we mostly care about Idle and Attack states
        // with Chase used for target acquisition/rotation

        // Attack state has highest priority when facing target
        if (currentTarget != null)
        {
            // Update last detection time since we have a target
            lastDetectionTime = Time.time;

            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            float angle = Vector3.Angle(turretPivot != null ? turretPivot.forward : transform.forward, directionToTarget);

            if (angle < attackAngleThreshold)
            {
                weights.Add(new StateWeight(EnemyState.Attack, 5.0f));
            }
            else
            {
                // Need to rotate to target
                weights.Add(new StateWeight(EnemyState.Chase, 4.0f));
            }
        }

        // Idle state when nothing else to do
        weights.Add(new StateWeight(EnemyState.Idle, 0.5f));

        // Minimal weights for retreat and strafe since they're not used
        weights.Add(new StateWeight(EnemyState.Retreat, 0.01f));
        weights.Add(new StateWeight(EnemyState.Strafe, 0.01f));

        return weights;
    }
    private void CheckForCombatSounds()
    {
        // Detect nearby combat and become alerted
        if (!isAlerted)
        {
            Collider[] nearbyEntities = Physics.OverlapSphere(transform.position, alertRadius, targetsLayerMask);
            foreach (Collider entity in nearbyEntities)
            {
                // If there's a player or other interesting object nearby, become alerted
                if (entity.CompareTag("Player") || (priorityTags != null && System.Array.Exists(priorityTags, tag => entity.CompareTag(tag))))
                {
                    isAlerted = true;
                    lastDetectionTime = Time.time; // Update detection time when alerted
                    break;
                }
            }
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        // Reset states when reactivated
        isAlerted = false;
        isRepositioning = false;
        isSinking = false;
        isUnderground = false;
        lastDetectionTime = Time.time;

        // Disable NavMeshAgent movement upon enabling
        if (agent != null)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
    }
    public void SetAlerted(bool alerted)
    {
        isAlerted = alerted;
        if (isAlerted)
        {
            lastDetectionTime = Time.time; // Update detection time when alerted

            if (currentState == EnemyState.Idle)
            {
                // Force target detection when alerted
                DetectTargets();
                if (currentTarget != null)
                {
                    SetState(EnemyState.Chase);
                }
            }
        }
    }
    private IEnumerator RepositionCoroutine()
    {
        // Start repositioning process
        isRepositioning = true;

        // First, sink into the ground
        yield return StartCoroutine(SinkIntoGround());

        // Find new position
        FindNewPosition();

        // Move to new position (underground)
        yield return StartCoroutine(MoveUnderground());

        // Rise at new position
        yield return StartCoroutine(RiseFromGround());

        // Repositioning complete
        isRepositioning = false;

        // Detect targets and set state
        DetectTargets();
        SetRandomState();

        // Reset last detection time
        lastDetectionTime = Time.time;
    }
    private IEnumerator SinkIntoGround()
    {
        // Start sinking animation
        isSinking = true;

        // Play sink animation if available
        if (animator != null)
        {
            animator.SetTrigger("Sink");
        }

        // Spawn VFX
        if (sinkVFXPrefab != null && PoolManager.Instance != null)
        {
            if (PoolManager.Instance.vfxPool.TryGetValue(sinkVFXPrefab, out VFXPool pool))
            {
                VFXPoolingBehavior vfx = pool.Get(transform.position, Quaternion.identity);
                vfx.Initialize(pool);
            }
        }

        // Disable any active attacks
        SetIsAttacking(false);

        // Gradually sink
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y - 2f, startPos.z); // Sink 2 units below ground

        float elapsed = 0;
        while (elapsed < sinkingDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / sinkingDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        // Set underground state
        isSinking = false;
        isUnderground = true;

        // Disable colliders while underground
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }
    private void FindNewPosition()
    {
        // Find a suitable new position within repositionRadius
        bool positionFound = false;
        int attempts = 0;
        const int maxAttempts = 30;

        while (!positionFound && attempts < maxAttempts)
        {
            attempts++;

            // Generate random position within radius
            Vector2 randomCircle = Random.insideUnitCircle * repositionRadius;
            Vector3 testPosition = originalPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Check if position is on NavMesh
            if (NavMesh.SamplePosition(testPosition, out NavMeshHit navHit, 5.0f, NavMesh.AllAreas))
            {
                // Check for obstacles
                bool obstacleFound = Physics.CheckSphere(navHit.position, minDistanceFromObstacles, repositionLayerMask);

                if (!obstacleFound)
                {
                    // Position is valid
                    targetRepositionPoint = navHit.position;
                    targetRepositionPoint.y = originalYPosition; // Maintain original Y height
                    positionFound = true;
                }
            }
        }

        // If no position found, just use a fallback position
        if (!positionFound)
        {
            targetRepositionPoint = originalPosition;
        }
    }
    private IEnumerator MoveUnderground()
    {
        // Enable agent while underground for path calculation
        if (agent != null)
        {
            agent.updatePosition = true;
            agent.isStopped = false;

            // Set destination
            agent.SetDestination(targetRepositionPoint);

            // Move underground
            Vector3 startPos = transform.position;
            Vector3 moveDirection = (targetRepositionPoint - startPos).normalized;

            float distanceToTarget = Vector3.Distance(transform.position, targetRepositionPoint);
            float totalMoveTime = distanceToTarget / movementSpeed;
            float elapsed = 0;

            while (elapsed < totalMoveTime)
            {
                // Move directly underground (not using agent for actual movement)
                Vector3 newPos = Vector3.Lerp(startPos, new Vector3(targetRepositionPoint.x, startPos.y, targetRepositionPoint.z), elapsed / totalMoveTime);
                transform.position = newPos;

                elapsed += Time.deltaTime;
                yield return null;

                // Break if we're close enough
                if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                   new Vector3(targetRepositionPoint.x, 0, targetRepositionPoint.z)) < 0.5f)
                {
                    break;
                }
            }

            // Set final position (but still underground)
            transform.position = new Vector3(targetRepositionPoint.x, startPos.y, targetRepositionPoint.z);

            // Disable agent updates again
            agent.isStopped = true;
            agent.updatePosition = false;
        }
        else
        {
            // Direct movement without agent
            Vector3 startPos = transform.position;
            float distanceToTarget = Vector3.Distance(
                new Vector3(startPos.x, 0, startPos.z),
                new Vector3(targetRepositionPoint.x, 0, targetRepositionPoint.z));

            float totalMoveTime = distanceToTarget / movementSpeed;
            float elapsed = 0;

            while (elapsed < totalMoveTime)
            {
                Vector3 newPos = Vector3.Lerp(startPos,
                    new Vector3(targetRepositionPoint.x, startPos.y, targetRepositionPoint.z),
                    elapsed / totalMoveTime);

                transform.position = newPos;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Set final position (but still underground)
            transform.position = new Vector3(targetRepositionPoint.x, startPos.y, targetRepositionPoint.z);
        }
    }
    private IEnumerator RiseFromGround()
    {
        // Prepare to rise
        isUnderground = false;
        isSinking = true; // Reusing sink state for rising animation

        // Re-enable colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        // Calculate rise positions
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, originalYPosition, startPos.z);

        // Spawn VFX before rising
        if (sinkVFXPrefab != null && PoolManager.Instance != null)
        {
            if (PoolManager.Instance.vfxPool.TryGetValue(sinkVFXPrefab, out VFXPool pool))
            {
                VFXPoolingBehavior vfx = pool.Get(new Vector3(startPos.x, endPos.y, startPos.z), Quaternion.identity);
                vfx.Initialize(pool);
            }
        }

        // Gradually rise
        float elapsed = 0;
        while (elapsed < sinkingDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / sinkingDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        // Play rise animation
        if (animator != null)
        {
            animator.SetTrigger("Rise");
            yield return new WaitForSeconds(risingAnimationLength);
        }

        // Reset state
        isSinking = false;
    }
    // Override to handle our own rising animation
    public override void TriggerRiseAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Rise");

            // Disable movement during animation
            agent.isStopped = true;

            // Just detect targets after animation time
            Invoke("ActivateAfterRising", risingAnimationLength);
        }
    }
    private void ActivateAfterRising()
    {
        DetectTargets();
        SetRandomState();
        lastDetectionTime = Time.time;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        // Stop all repositioning when disabled
        StopAllCoroutines();
        isRepositioning = false;
        isSinking = false;
        isUnderground = false;

        // Re-enable any disabled colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }
}
