using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanleyControlScript : MonoBehaviour
{
    private const string enemyTag = "Enemy";
    [HideInInspector]
    public float
        moveSpeed,
        turnSpeed,
        changeDirectionInterval,
        detectionRadius,
        randomMovementWeight,
        myceliaGenerationRate,
        myceliaGenerationTickRate;

    private float moneyGenerationTimer = 0f;

    [SerializeField] GameObject[] StanleyVisuals;
    [SerializeField] private GameObject parentStructure;
    [Header("Enemy Avoidance")]
    [SerializeField] private LayerMask enemyLayers; // Layer for walls/obstacles
    [SerializeField] private float enemyInfluenceWeight = 2;
    [SerializeField] private float panicDistance = 5f; // Distance at which Stanley panics and flees at maximum speed
    [SerializeField] private float panicSpeedMultiplier = 1.5f; // How much faster Stanley moves when panicking


    [Header("Boundary Settings")]
    [SerializeField] private float wallAvoidanceDistance = 5f; // Distance to start avoiding walls
    [SerializeField] private LayerMask obstacleLayers; // Layer for walls/obstacles
    [SerializeField] private float wallAvoidanceWeight = 2f; // How strongly to avoid walls
    [SerializeField] private GameObject deathVFX;
    private AudioSource audioPlayer;
    private Vector3 currentDirection;

    void Start()
    {
        audioPlayer = GetComponent<AudioSource>();
        // Set initial direction to forward
        currentDirection = Vector3.forward;
        // Start the direction-changing process
        InvokeRepeating(nameof(UpdateDirection), 1f, changeDirectionInterval);

        // Get the layer that obstacles are on
        obstacleLayers = LayerMask.GetMask("Obstacle");
    }
    void Update()
    {
        // Check for walls before moving
        if (!IsBlockedByWall(currentDirection))
        {
            transform.Translate(moveSpeed * Time.deltaTime * currentDirection, Space.World);
        }
        // Generate money if Stanley is active
        if (GameManager.Instance.waveManager.wavePhase == WaveManager.WavePhase.Started)
        {
            GenerateMycelia();
        }
        // Smooth rotation towards movement direction
        Quaternion targetRotation = Quaternion.LookRotation(currentDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
    private void GenerateMycelia()
    {
        // Increment timer
        moneyGenerationTimer += Time.deltaTime;

        // Check if it's time to generate money
        if (moneyGenerationTimer >= myceliaGenerationTickRate)
        {
            // Calculate money to generate based on time passed
            float moneyToAdd = myceliaGenerationRate * myceliaGenerationTickRate;

            // Add money
            GameManager.Instance.IncreaseMycelia(moneyToAdd);

            // Reset timer
            moneyGenerationTimer = 0f;
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

        // Check if enemies are very close
        bool isPanicking = IsEnemyTooClose(panicDistance);

        // Combine all influences with their weights
        currentDirection = (
            enemyDirection * (isPanicking ? enemyInfluenceWeight * 2 : enemyInfluenceWeight) +
            randomDirection * (isPanicking ? 0 : randomMovementWeight) + // No random movement when panicking
            wallAvoidance * wallAvoidanceWeight
        ).normalized;

        // Temporarily increase speed when panicking
        float currentSpeed = isPanicking ? moveSpeed * panicSpeedMultiplier : moveSpeed;

        currentDirection.y = 0f; // Keep movement on the horizontal plane
    }
    // Helper method to check if any enemy is within panic distance
    private bool IsEnemyTooClose(float distance)
    {
        Collider[] nearbyColliders = new Collider[5]; // Small array since we only need to find one
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, distance, nearbyColliders, enemyLayers);
        return numColliders > 0;
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

        // Pre-allocate an array to store results
        Collider[] nearbyColliders = new Collider[20];

        // Use OverlapSphereNonAlloc to avoid allocating a new array each time and reduce garbage collection
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, nearbyColliders, enemyLayers);

        if (numColliders == 0)
            return GetRandomDirection(); // If no enemies nearby, return random direction

        // Loop through only the colliders that were found
        for (int i = 0; i < numColliders; i++)
        {
            // Calculate direction AWAY from enemy (note the reversed order of subtraction)
            Vector3 directionAwayFromEnemy = (transform.position - nearbyColliders[i].transform.position).normalized;
            float distanceToEnemy = Vector3.Distance(transform.position, nearbyColliders[i].transform.position);

            // Enemies closer by have more influence
            float influence = 1f - (distanceToEnemy / detectionRadius);
            enemyInfluence += directionAwayFromEnemy * influence;
        }

        return enemyInfluence.normalized;
    }
    protected virtual void SpawnVFX(Vector3 position, Quaternion rotation)
    {
        if (deathVFX != null)
        {
            GameObject vfx = Instantiate(deathVFX, position, rotation);
            Destroy(vfx, 2f); // Incase it doesnt auto destroy
        }
    }
    public void UpdateVisual(int index)
    {
        StanleyVisuals[index].SetActive(true);
        if (index > 0)
            StanleyVisuals[index - 1].SetActive(false);
    }
    private void OnDrawGizmosSelected()
    {
        // Show detection radius
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
