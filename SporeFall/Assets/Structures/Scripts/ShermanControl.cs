using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShermanControl : MonoBehaviour
{
    // Movement speed
    public float moveSpeed = 2f;
    // How fast the object changes direction
    public float turnSpeed = 1f;
    // How often to change direction
    public float changeDirectionInterval = 2f;
    // Damage dealt to enemies on contact
    public int damage = 100;
    // Detection radius for nearby enemies
    public float detectionRadius = 10f;
    // Weight for enemy influence on direction (higher = more attracted to enemies)
    public float enemyInfluenceWeight = 2f;
    // Weight for random movement (higher = more random movement)
    public float randomMovementWeight = 1f;
    public string enemyTag = "Enemy";
    private Vector3 currentDirection;
    [Header("Explosion Settings")]
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float explosionRadius = 10f;
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] private AudioClip explosionSF;
    [SerializeField] private GameObject explosionVF;
    private AudioSource audioPlayer;
    void Start()
    {
        audioPlayer = GetComponent<AudioSource>();
        // allow sherman to fly out of house
        // Set initial direction to forward
        currentDirection = transform.forward;
        // Start the direction-changing process
        InvokeRepeating("UpdateDirection", 1f, changeDirectionInterval);

        // Get the layer that enemies are on
        damageableLayers = LayerMask.GetMask("Enemy");
    }

    void Update()
    {
        // Move in the current direction
        transform.Translate(currentDirection * moveSpeed * Time.deltaTime, Space.World);

        // Smooth rotation towards movement direction
        Quaternion targetRotation = Quaternion.LookRotation(currentDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
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
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius, damageableLayers);

        if (nearbyColliders.Length == 0)
            return GetRandomDirection(); // If no enemies nearby, return random direction

        // Calculate direction based on nearby enemies
        foreach (Collider enemyCollider in nearbyColliders)
        {
            Vector3 directionToEnemy = (enemyCollider.transform.position - transform.position).normalized;
            float distanceToEnemy = Vector3.Distance(transform.position, enemyCollider.transform.position);

            // Enemies closer by have more influence
            float influence = 1f - (distanceToEnemy / detectionRadius);
            enemyInfluence += directionToEnemy * influence;
        }

        return enemyInfluence.normalized;
    }

    private void UpdateDirection()
    {
        Vector3 enemyDirection = GetEnemyInfluenceDirection();
        Vector3 randomDirection = GetRandomDirection();

        // Combine random and enemy-influenced directions using weights
        currentDirection = (enemyDirection * enemyInfluenceWeight + randomDirection * randomMovementWeight).normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(enemyTag))
        {
            // Play effects
            PlaySFX();
            SpawnVFX(transform.position, Quaternion.identity);

            // Find all colliders in explosion radius
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayers);

            foreach (Collider hit in hitColliders)
            {
                // Calculate distance for damage falloff
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float damageMultiplier = damageFalloff.Evaluate(distance / explosionRadius);

                // Apply damage if object has IDamageable interface
                Damageable damageable = hit.GetComponent<Damageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage * damageMultiplier);
                }
                // play audio before destroying
                /*Damageable enemy = collision.gameObject.GetComponent<Damageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damageAmount);
                    Destroy(gameObject);
                }*/
            }

            Destroy(gameObject, explosionSF.length);
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
    // Optional: Visualize the detection radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}