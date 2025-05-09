using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class OctoBoss : BaseEnemy
{
    [Header("Tentacle Settings")]
    [SerializeField] private List<TentacleEnemy> tentacles = new();
    [Header("Main Body Visuals")]
    [SerializeField] private Renderer bodyRenderer;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip shieldedHitSound;
    [Header("Phase Damage")]
    [SerializeField] private float damageReductionMultiplier = 0.25f; // 75% damage reduction when more than half tentacles alive
    [SerializeField] private float bodyVulnerableMultiplier = 1.5f;   // 50% more damage when vulnerable

    private bool isVulnerable = false;
    private bool isDying = false; // Flag to prevent tentacles from modifying the list during boss death


    [Header("Stationary Settings")]
    [SerializeField] private Transform mainBody;
    public Transform CurrentTarget => currentTarget;
    private float initialTentacleCount = 0;

    protected override void Awake()
    {
        base.Awake();
    }
    public override void Initialize()
    {
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
        }
    }
    private void CheckVulnerabilityState()
    {
        if(tentacles.Count <= initialTentacleCount / 2)
        {
            isVulnerable = true;
            RemoveShield();
        }
        else
        {
            isVulnerable = false;
        }
    }
    private void RemoveShield()
    {
        // Change material based on vulnerability state
        if (bodyRenderer != null)
        {
            Material[] materials = bodyRenderer.materials;
            Destroy(materials[1]);
            materials[1] = null;
            bodyRenderer.materials = materials;
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
    public void PlayHitSoundFX()
    {
        if (isVulnerable && hitSound != null)
            audioSource.PlayOneShot(hitSound);
        else if (shieldedHitSound != null)
            audioSource.PlayOneShot(shieldedHitSound);
    }
    public void AddTentacle(TentacleEnemy tentacle)
    {
        // Prevent Duplicates
        if (!tentacles.Contains(tentacle))
        {
            tentacles.Add(tentacle);
            initialTentacleCount++;
        }
    }
    public void RemoveTentacle(TentacleEnemy tentacle)
    {
        // Skip removal if the boss is already in its death sequence
        if (isDying)
            return;

        tentacles.Remove(tentacle);
        CheckVulnerabilityState();

    }
    public override void Die()
    {
        isDying = true;

        // Clean up any null references first
        tentacles.RemoveAll(item => item == null);

        // Create a copy of the tentacles list to avoid modification during iteration
        List<TentacleEnemy> tentaclesToDestroy = new List<TentacleEnemy>(tentacles);

        // Destroy any remaining tentacles first
        foreach (var tentacle in tentaclesToDestroy)
        {
            if (tentacle != null)
                tentacle.Die();
        }

        // Clear the original list
        tentacles.Clear();

        // Then handle the boss death
        base.Die();
    }
}
