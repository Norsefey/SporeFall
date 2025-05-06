using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LilyRepairBot : MonoBehaviour
{
    public bool isActive = false;
    // upgradeable Stats
    public float patrolRange = 10f;
    public float moveSpeed = 2f;
    public float repairRate = 7f;
    public GameObject[] lilyVisuals;
    // Non-upgradeable Stats
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float minDistanceToTarget = 1.5f;

    [SerializeField] private Transform shopStructure;

    [SerializeField] private LayerMask structureHPLayer;
    private StructureHP currentTarget;
    private Coroutine repairCoroutine;
    private NavMeshAgent navAgent;
    private float maxHealTime = 0f;
    private float healTimer = 0f;
    private bool isRepairing = false;

    private enum BotState { Roaming, MovingToTarget, Repairing }
    private BotState currentState;
   
    private void SetupNavStats()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("NavMeshAgent component missing from LilyRepairBot!");
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.acceleration = 8;
            navAgent.stoppingDistance = minDistanceToTarget;
        }
    }
    private void Update()
    {
        if (!isActive) return;

        switch (currentState)
        {
            case BotState.Roaming:
                // Check if we've reached the destination or if we have no path
                if (!navAgent.pathPending && (navAgent.remainingDistance <= navAgent.stoppingDistance ||
                    navAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
                    !navAgent.hasPath))
                {
                    SetNewRandomTarget();
                }
                DetectStructures();
                break;

            case BotState.MovingToTarget:
                if (currentTarget == null)
                {
                    // Target was destroyed or no longer exists
                    currentState = BotState.Roaming;
                    SetNewRandomTarget();
                    return;
                }

                // Check if we've reached the target
                if (!navAgent.pathPending && navAgent.remainingDistance <= minDistanceToTarget)
                {
                    StartRepairing();
                }
                break;

            case BotState.Repairing:
                // Repair happens in coroutine
                break;
        }
    }
    private void SetNewRandomTarget()
    {
        // Find a random point on NavMesh within roaming radius of shop
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
        randomDirection += shopStructure.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRange, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
        }
        else
        {
            // If we couldn't find a valid point, try again
            StartCoroutine(RetryFindRandomPoint());
        }
    }
    private IEnumerator RetryFindRandomPoint()
    {
        yield return new WaitForSeconds(0.5f);
        SetNewRandomTarget();
    }
    private void DetectStructures()
    {
        // Pre-allocate an array to store results
        Collider[] colliderResults = new Collider[20];

        // Use OverlapSphereNonAlloc to avoid allocating a new array each time and reduce garbage collection
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, colliderResults, structureHPLayer);

        // Loop through only the colliders that were found
        for (int i = 0; i < numColliders; i++)
        {
            StructureHP structure = colliderResults[i].GetComponent<StructureHP>();
            if (structure != null && structure.CurrentHP < structure.MaxHP)
            {
                // Found damaged structure
                currentTarget = structure;
                currentState = BotState.MovingToTarget;
                // Set navigation destination to structure
                navAgent.SetDestination(structure.transform.position);
                return;
            }
        }
    }
    private void StartRepairing()
    {
        isRepairing = true;
        currentState = BotState.Repairing;

        // Stop movement
        navAgent.isStopped = true;

        // Calculate max time based on structure's missing HP
        float missingHP = currentTarget.MaxHP - currentTarget.CurrentHP;
        maxHealTime = missingHP / repairRate;
        healTimer = 0f;

        // Start repair coroutine
        if (repairCoroutine != null)
        {
            StopCoroutine(repairCoroutine);
        }
        repairCoroutine = StartCoroutine(RepairStructure());
    }
    private IEnumerator RepairStructure()
    {
        while (isRepairing)
        {
            healTimer += Time.deltaTime;

            // Repair structure
            if (currentTarget != null)
            {
                currentTarget.RestoreHP(repairRate * Time.deltaTime);

                // Check if finished repairing
                if (currentTarget.CurrentHP >= currentTarget.MaxHP || healTimer >= maxHealTime)
                {
                    FinishRepairing();
                    break;
                }
            }
            else
            {
                // Target no longer exists
                FinishRepairing();
                break;
            }

            yield return null;
        }
    }
    private void FinishRepairing()
    {
        isRepairing = false;
        currentTarget = null;
        currentState = BotState.Roaming;

        // Resume movement
        navAgent.isStopped = false;

        SetNewRandomTarget();
    }
    public void ActivateBot(Transform startPos)
    {
        SetupNavStats();

        transform.position = startPos.position;
        navAgent.Warp(startPos.position); // Ensure NavMeshAgent position is updated
        isActive = true;
        currentState = BotState.Roaming;

        transform.GetChild(0).gameObject.SetActive(true);
/*        // Resume agent if it was stopped
        navAgent.isStopped = false;
        SetNewRandomTarget();*/
    }
    public void DeactivateBot()
    {
        isActive = false;
        //transform.GetChild(0).gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
        // Stop movement
        if (navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }

        if (repairCoroutine != null)
        {
            StopCoroutine(repairCoroutine);
        }

        currentTarget = null;
        isRepairing = false;
    }
    public void SetShopStructure(Transform shop)
    {
        shopStructure = shop;
    }
    public void UpdateVisual(int index)
    {
        lilyVisuals[index].SetActive(true);
        if (index > 0)
            lilyVisuals[index - 1].SetActive(false);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (shopStructure != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(shopStructure.position, patrolRange);
        }
    }
}
