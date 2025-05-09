using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public float protectionRange = 5f;
    public float damageReduction = 0.25f;

    [SerializeField] private LayerMask detectionLayer;
    [Range(0, 1)]
    [SerializeField] private float checkInterval = 0.5f; // How often to check for allies

    private List<Damageable> damageablesProtecting = new List<Damageable>();
    private bool isDestroyed = false;

    private void Start()
    {
        // Start the repeating check for allies
        InvokeRepeating(nameof(DetectNearbyAllies), 0f, checkInterval);
    }

    private void OnDestroy()
    {
        // Reset damage modifiers when wall is destroyed
        ResetAllProtections();
        isDestroyed = true;
    }

    private void OnDisable()
    {
        // Reset damage modifiers when wall is disabled
        ResetAllProtections();
    }

    private void DetectNearbyAllies()
    {
        if (isDestroyed)
            return;

        // Pre-allocate an array to store results
        Collider[] colliderResults = new Collider[20];

        // Use OverlapSphereNonAlloc to avoid allocating a new array each time and reduce garbage collection
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, protectionRange, colliderResults, detectionLayer);

        // Track which damageables are still in range
        HashSet<Damageable> currentlyInRange = new HashSet<Damageable>();

        // Loop through only the colliders that were found
        for (int i = 0; i < numColliders; i++)
        {
            Damageable hpComponent = colliderResults[i].GetComponent<Damageable>();
            if (hpComponent != null)
            {
                currentlyInRange.Add(hpComponent);

                // Apply damage reduction if not already protected
                if (!damageablesProtecting.Contains(hpComponent))
                {
                    hpComponent.damageReduction = damageReduction;
                    damageablesProtecting.Add(hpComponent);
                }
            }
        }

        // Check for any damageables that are no longer in range
        for (int i = damageablesProtecting.Count - 1; i >= 0; i--)
        {
            Damageable damageable = damageablesProtecting[i];
            if (damageable == null || !currentlyInRange.Contains(damageable))
            {
                // Reset damage modifier to default (0) and remove from our list
                if (damageable != null)
                {
                    damageable.damageReduction = 0f;
                }
                damageablesProtecting.RemoveAt(i);
            }
        }
    }

    private void ResetAllProtections()
    {
        // Remove damage reduction from all protected entities
        foreach (var damageable in damageablesProtecting)
        {
            if (damageable != null)
            {
                damageable.damageReduction = 0f;
            }
        }
        damageablesProtecting.Clear();
    }

    // Visualize the protection range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, protectionRange);
    }
}
