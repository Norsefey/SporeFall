using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShermanControl : MonoBehaviour
{
    private const string enemyTag = "Enemy";
    [HideInInspector]
    public float
        moveSpeed, 
        turnSpeed, 
        changeDirectionInterval, 
        damage, 
        detectionRadius,
        enemyInfluenceWeight,
        randomMovementWeight, 
        explosionRadius;
    [HideInInspector]
    public AnimationCurve damageFalloff;
    [SerializeField] GameObject[] shermanVisuals;
    [SerializeField] private ShermanStructureControls parentStructure;
    [Header("Boundary Settings")]
    [SerializeField] private float wallAvoidanceDistance = 5f; // Distance to start avoiding walls
    [SerializeField] private LayerMask obstacleLayers; // Layer for walls/obstacles
    [SerializeField] private float wallAvoidanceWeight = 2f; // How strongly to avoid walls

    [Header("Explosion Settings")]
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private AudioClip explosionSF;
    [SerializeField] private GameObject explosionVF;
    private AudioSource audioPlayer;
    private Vector3 currentDirection;
    private bool active = true;
    void Start()
    {
        active = true;
        audioPlayer = GetComponent<AudioSource>();
        // allow sherman to fly out of house
        // Set initial direction to forward
        currentDirection = Vector3.forward;
        // Start the direction-changing process
        InvokeRepeating(nameof(UpdateDirection), 1f, changeDirectionInterval);

        // Get the layer that enemies are on
        damageableLayers = LayerMask.GetMask("Enemy");
        obstacleLayers = LayerMask.GetMask("Obstacle");
    }
    void Update()
    {
        if (active)
        {
            // Check for walls before moving
            if (!IsBlockedByWall(currentDirection))
            {
                transform.Translate(moveSpeed * Time.deltaTime * currentDirection, Space.World);
            }

            // Smooth rotation towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(currentDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
    private bool IsBlockedByWall(Vector3 direction)
    {
        // Cast a ray in the movement direction to check for walls
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, wallAvoidanceDistance, obstacleLayers))
        {
            // If we're about to hit a wall, adjust the direction
            currentDirection = Vector3.Reflect(currentDirection, hit.normal).normalized;
            return true;
        }
        return false;
    }
    private Vector3 GetWallAvoidanceDirection()
    {
        Vector3 avoidanceDirection = Vector3.zero;

        // Cast rays in multiple directions to detect nearby walls
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, wallAvoidanceDistance, obstacleLayers))
            {
                // Add a force away from the wall, stronger when closer
                float distance = hit.distance;
                float strength = 1f - (distance / wallAvoidanceDistance);
                avoidanceDirection -= hit.normal * strength;
            }
        }

        return avoidanceDirection.normalized;
    }
    private void UpdateDirection()
    {
        Vector3 enemyDirection = GetEnemyInfluenceDirection();
        Vector3 randomDirection = GetRandomDirection();
        Vector3 wallAvoidance = GetWallAvoidanceDirection();

        // Combine all influences with their weights
        currentDirection = (
            enemyDirection * enemyInfluenceWeight +
            randomDirection * randomMovementWeight +
            wallAvoidance * wallAvoidanceWeight
        ).normalized;

        currentDirection.y = 0f; // Keep movement on the horizontal plane
    }
    // This function generates a random direction
    private Vector3 GetRandomDirection()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        return new Vector3(randomX, 0, randomZ).normalized;
    }
    private Vector3 GetEnemyInfluenceDirection()
    {
        Vector3 enemyInfluence = Vector3.zero;
        /*        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius, damageableLayers);
        */

        // Pre-allocate an array to store results
        Collider[] nearbyColliders = new Collider[20];

        // Use OverlapSphereNonAlloc to avoid allocating a new array each time and reduce garbage collection
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, nearbyColliders, damageableLayers);
        
        if (numColliders == 0)
            return GetRandomDirection(); // If no enemies nearby, return random direction
        
        // Loop through only the colliders that were found
        for (int i = 0; i < numColliders; i++)
        {
            Vector3 directionToEnemy = (nearbyColliders[i].transform.position - transform.position).normalized;
            float distanceToEnemy = Vector3.Distance(transform.position, nearbyColliders[i].transform.position);

            // Enemies closer by have more influence
            float influence = 1f - (distanceToEnemy / detectionRadius);
            enemyInfluence += directionToEnemy * influence;
        }

        return enemyInfluence.normalized;
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag(enemyTag) && active)
        {

            // Play effects
            PlaySFX();
            SpawnVFX(transform.position, Quaternion.identity);

       /*     // Find all colliders in explosion radius
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayers);*/

            // Pre-allocate an array to store results
            Collider[] colliderResults = new Collider[20];

            // Use OverlapSphereNonAlloc to avoid allocating a new array each time and reduce garbage collection
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, colliderResults, damageableLayers);

            // Loop through only the colliders that were found
            for (int i = 0; i < numColliders; i++)
            {
                // Calculate distance for damage falloff
                float distance = Vector3.Distance(transform.position, colliderResults[i].transform.position);
                float damageMultiplier = damageFalloff.Evaluate(distance / explosionRadius);

                // Apply damage if object has IDamageable interface
                if (colliderResults[i].TryGetComponent<Damageable>(out var damageable))
                {
                    damageable.TakeDamage(damage * damageMultiplier);
                }
            }
            StartCoroutine(parentStructure.ResetShermanBots());
        }
    }
    protected virtual void SpawnVFX(Vector3 position, Quaternion rotation)
    {
        if (explosionVF != null)
        {
            GameObject vfx = Instantiate(explosionVF, position, rotation);
            Destroy(vfx, 2f); // Incase it doesnt auto destroy
        }
    }
    protected virtual void PlaySFX()
    {
        if (explosionSF != null && audioPlayer != null)
        {
            audioPlayer.PlayOneShot(explosionSF);
        }
    }

    public void DeactivateSherman()
    {
        active = false;
        transform.GetChild(0).gameObject.SetActive(false);

    }
    public void ActivateSherman()
    {
        active = true;
        transform.GetChild(0).gameObject.SetActive(true);
    }
    public void UpdateVisual(int index)
    {
        shermanVisuals[index].SetActive(true);
        if(index > 0)
            shermanVisuals[index - 1].SetActive(false);
    }
    private void OnDrawGizmosSelected()
    {
        // Show enemy detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Show wall detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, wallAvoidanceDistance);

        // Show current direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, currentDirection * 3f);
    }
}