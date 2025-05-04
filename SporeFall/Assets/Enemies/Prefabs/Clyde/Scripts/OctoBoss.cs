using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctoBoss : BaseEnemy
{
    [Header("Tentacle Settings")]
    [SerializeField] private TentaclePart[] tentacles;
    [SerializeField] private float damageReductionMultiplier = 0.25f; // 75% damage reduction when more than half tentacles alive
    [SerializeField] private float bodyVulnerableMultiplier = 1.5f;   // 50% more damage when vulnerable

    private bool isVulnerable = false;
    private int initialTentacleCount;

    [Header("Stationary Settings")]
    [SerializeField] private Transform mainBody;
    public Transform CurrentTarget => currentTarget;

    protected override void Awake()
    {
        base.Awake();
        initialTentacleCount = tentacles.Length;

        // Initialize tentacles
        foreach (var tentacle in tentacles)
        {
            tentacle.Initialize(this);
        }
    }
    public override void Initialize()
    {
        Debug.LogWarning("Reseting Attack: ");

        ResetState();
        StartCoroutine(PeriodicTargetDetection(4));
        // Since this is stationary, disable NavMesh movement
        if (agent != null)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
    }
    protected override void Update()
    {
        UpdateStateTimer();

        if (!isAttacking)
        {
            UpdateCurrentState();
            // Check vulnerability state
            CheckVulnerabilityState();
        }
    }
    private void CheckVulnerabilityState()
    {
        int activeTentacles = 0;
        foreach (var tentacle in tentacles)
        {
            if (tentacle.IsActive())
                activeTentacles++;
        }

        // More than half tentacles destroyed = vulnerable
        bool newVulnerableState = activeTentacles <= (initialTentacleCount / 2);

        if (newVulnerableState != isVulnerable)
        {
            isVulnerable = newVulnerableState;
            OnVulnerabilityStateChanged();
        }
    }
    private void OnVulnerabilityStateChanged()
    {
        if (isVulnerable)
        {
            if (animator != null)
                animator.SetBool("Vulnerable", true);

            // Could play effects, change materials, etc.
            Debug.Log("Boss is now vulnerable!");
        }
        else
        {
            if (animator != null)
                animator.SetBool("Vulnerable", false);
        }
    }

    // Modify damage taking based on vulnerability
    public float CalculateDamageMultiplier()
    {
        if (isVulnerable)
            return bodyVulnerableMultiplier; // Take extra damage when vulnerable
        else
            return damageReductionMultiplier; // Take reduced damage when protected
    }

    // Overriding as we don't need movement-related behavior
    protected override void UpdateIdleState()
    {
        base.UpdateIdleState();
        
        // for when current target is destroyed find a new target
        if (currentTarget == trainWall)
        {
            DetectTargets();
        }
    }
    protected override void UpdateChaseState(float distanceToTarget)
    {
        // Can't chase since we're stationary, so just go to attack state if in range
        if (distanceToTarget <= stoppingDistance)
        {
            SetState(EnemyState.Attack);
        }
        else
        {
            // Can't get closer, so just go to idle
            SetState(EnemyState.Idle);
        }
    }
    protected override void UpdateStrafeState()
    {
        // Can't strafe since we're stationary
        SetState(EnemyState.Idle);
    }
    protected override List<StateWeight> CalculateStateWeights()
    {
        List<StateWeight> weights = new();
        float distanceToTarget = currentTarget ? Vector3.Distance(transform.position, currentTarget.position) : float.MaxValue;

        // Stationary enemy prioritizes attacking if in range, otherwise idle
        float attackWeight = distanceToTarget <= stoppingDistance ? 5.0f : 0.1f;
        weights.Add(new StateWeight(EnemyState.Attack, attackWeight));

        // Lower priority for other states
        weights.Add(new StateWeight(EnemyState.Idle, 1.0f));
        weights.Add(new StateWeight(EnemyState.Chase, 0)); // never chase
        weights.Add(new StateWeight(EnemyState.Strafe, 0)); // never strafe

        return weights;
    }
    public void OnTentacleDestroyed()
    {
        CheckVulnerabilityState();
    }
    public override void Die()
    {
        // Destroy any remaining tentacles first
        foreach (var tentacle in tentacles)
        {
            if (tentacle.IsActive())
                tentacle.DestroyTentacle();
        }

        // Then handle the boss death
        base.Die();
    }
}
