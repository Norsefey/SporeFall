using System.Collections;
using UnityEngine;


// Projectile data structure
[System.Serializable]
public struct ProjectileData
{
    public float Speed;
    public float Damage;
    public float Lifetime;
    public bool UseGravity;
    public float ArcHeight;
    public bool CanBounce;
    public int MaxBounces;
    public float BounceDamageMultiplier;
    public bool targetedDirection;
    [HideInInspector]
    public Vector3 Direction;
    [HideInInspector]
    public Vector3 TargetPosition; // Added target position for arc calculations
}
public enum ProjectileType
{
    Standard,
    Explosive,
    DOT, 
    Corrupted,
    Spawner
}
public class ProjectileBehavior : MonoBehaviour
{
    private ProjectileData data;
    private ProjectilePool pool;
    private float elapsedTime;
    private Rigidbody rb;
    private int bounceCount;
    private float damage;

    [Header("General Settings")]
    [SerializeField] private ProjectileType type;
    [SerializeField] protected LayerMask hitLayers;
    [SerializeField] private GameObject vfxPrefab;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioPlayer;
    public AudioClip bulletSound; // Assign the bullet sound clip in the inspector
    [Range(0f, 1f)] public float bulletSoundVolume = 0.5f; // Set the default volume in the inspector

    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionRadius;
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("DOT Zone Settings")]
    [SerializeField] private bool createsDOTZone;
    [SerializeField] private GameObject dotZonePrefab;
    [SerializeField] private float dOTDuration;
    [SerializeField] private float dOTTickRate;
    [SerializeField] private float dOTDamagePerTick;
    [SerializeField] private float dOTRadius;
    [SerializeField] private float dOTCorruptionPerTick;

    [Header("Corruption Settings")]
    [SerializeField] private float corruptionAmount;

    [Header("Spawner Settings")]
    [SerializeField] private GameObject[] entitiesToSpawn;

    private Vector3 initialPosition;
    private float arcProgress = 0;
    private float arcDistance;

    // Added for collision detection in arc mode
    [SerializeField] private float collisionCheckRadius = 0.5f;
    private Vector3 previousPosition;
    public bool targetedDirection = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (audioPlayer != null)
            audioPlayer.volume = bulletSoundVolume;
    }

    private void Update()
    {
        previousPosition = transform.position;

        // For arcing projectiles
        if (targetedDirection)
        {
            arcProgress += data.Speed * Time.deltaTime / arcDistance;

            // Calculate position along the arc
            Vector3 linearPosition = Vector3.Lerp(initialPosition, data.TargetPosition, arcProgress);

            // Add arc height (parabolic motion)
            float heightOffset = data.ArcHeight * Mathf.Sin(arcProgress * Mathf.PI);
            transform.position = linearPosition + Vector3.up * heightOffset;

            // Rotate to face the direction of movement
            if (arcProgress < 1)
            {
                Vector3 nextPosition = Vector3.Lerp(initialPosition, data.TargetPosition, arcProgress + 0.01f);
                nextPosition += Vector3.up * (data.ArcHeight * Mathf.Sin((arcProgress + 0.01f) * Mathf.PI));
                transform.LookAt(nextPosition);
            }

            // Check for collisions manually for arc trajectories
            CheckForCollisions();

            // Terminate at end of arc
            if (arcProgress >= 1)
            {
                // Create explosion at landing point
                switch (type)
                {
                    case ProjectileType.Explosive:
                        HandleExplosiveAttack();
                        break;
                    case ProjectileType.DOT:
                        HandleDOTAttack();
                        break;
                    case ProjectileType.Spawner:
                        HandleSpawnerBehavior();
                        break;
                    default:
                        // For non-explosive projectiles, check for direct hits
                        Collider[] hits = Physics.OverlapSphere(transform.position, collisionCheckRadius, hitLayers);
                        foreach (var hit in hits)
                        {
                            if (hit.TryGetComponent<Damageable>(out var damageable))
                            {
                                ApplyDamage(damageable);
                            }
                        }
                        break;
                }
                ReturnToPool();
            }
        }
        else
        {
            // Only use direct position updates for non-gravity projectiles
            // For projectiles with gravity, we use the rigidbody velocity
            transform.position += data.Direction * data.Speed * Time.deltaTime;

            // Also check for collisions if we're manually updating position
            //CheckForCollisions();
        }
    }

    // Added method to check for collisions when using kinematic movement
    private void CheckForCollisions()
    {
        // Calculate movement direction
        Vector3 movementDirection = (transform.position - previousPosition).normalized;
        float movementDistance = Vector3.Distance(previousPosition, transform.position);

        if (movementDistance < 0.001f) return; // Skip if barely moving

        // Cast a sphere in the direction of movement
        RaycastHit hit;
        if (Physics.SphereCast(previousPosition, collisionCheckRadius, movementDirection,
                              out hit, movementDistance, hitLayers))
        {
            // Handle the collision similar to OnCollisionEnter
            Debug.Log("Projectile " + gameObject.name + " Hit: " + hit.collider.gameObject.name + " during arc");

            // Create a fake collision for compatibility with existing code
            Collision fakeCollision = CreateFakeCollision(hit);

            switch (type)
            {
                case ProjectileType.Standard:
                    HandleStandardAttack(fakeCollision);
                    break;
                case ProjectileType.Explosive:
                    HandleExplosiveAttack();
                    break;
                case ProjectileType.DOT:
                    HandleDOTAttack();
                    break;
                case ProjectileType.Corrupted:
                    HandleCorruptionAttack(fakeCollision);
                    break;
                case ProjectileType.Spawner:
                    HandleSpawnerBehavior();
                    break;
            }

            // Check if we should handle bounce or destroy
            HandleCollision(fakeCollision);
        }
    }

    // Helper method to create a fake collision from a raycast hit
    private Collision CreateFakeCollision(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        // Create a new Collision
        Collision collision = new Collision();

        // Use reflection to set private fields (this is a workaround since Collision is not normally constructable)
        System.Reflection.FieldInfo contactPointsField = typeof(Collision).GetField("m_ContactPoints",
                                                                                  System.Reflection.BindingFlags.Instance |
                                                                                  System.Reflection.BindingFlags.NonPublic);

        if (contactPointsField != null)
        {
            // Create a contact point
            ContactPoint contact = new ContactPoint();

            // Set contact point fields via reflection
            typeof(ContactPoint).GetField("m_Point", System.Reflection.BindingFlags.Instance |
                                         System.Reflection.BindingFlags.NonPublic)?.SetValue(contact, hit.point);

            typeof(ContactPoint).GetField("m_Normal", System.Reflection.BindingFlags.Instance |
                                         System.Reflection.BindingFlags.NonPublic)?.SetValue(contact, hit.normal);

            typeof(ContactPoint).GetField("m_ThisCollider", System.Reflection.BindingFlags.Instance |
                                         System.Reflection.BindingFlags.NonPublic)?.SetValue(contact, GetComponent<Collider>());

            typeof(ContactPoint).GetField("m_OtherCollider", System.Reflection.BindingFlags.Instance |
                                         System.Reflection.BindingFlags.NonPublic)?.SetValue(contact, hit.collider);

            // Set the contact points
            contactPointsField.SetValue(collision, new ContactPoint[] { contact });
        }

        // Set the transform and gameObject fields
        typeof(Collision).GetField("m_Transform", System.Reflection.BindingFlags.Instance |
                                 System.Reflection.BindingFlags.NonPublic)?.SetValue(collision, hit.transform);

        typeof(Collision).GetField("m_Rigidbody", System.Reflection.BindingFlags.Instance |
                                 System.Reflection.BindingFlags.NonPublic)?.SetValue(collision, hit.rigidbody);

        typeof(Collision).GetField("m_GameObject", System.Reflection.BindingFlags.Instance |
                                 System.Reflection.BindingFlags.NonPublic)?.SetValue(collision, hitObject);

        return collision;
    }

    public void Initialize(ProjectileData projectileData, ProjectilePool pool)
    {
        data = projectileData;
        damage = data.Damage;
        this.pool = pool;
        elapsedTime = 0f;
        bounceCount = 0;

        initialPosition = transform.position;
        targetedDirection = data.targetedDirection;

        if (targetedDirection)
        {
            arcDistance = Vector3.Distance(initialPosition, data.TargetPosition);
            arcProgress = 0f;

            // Disable rigidbody physics for arc trajectories but keep collider active
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // Make sure the direction is normalized
            if (data.Direction.magnitude > 0)
            {
                data.Direction = data.Direction.normalized;
            }
            else
            {
                data.Direction = (data.TargetPosition - initialPosition).normalized;
            }

            // Store initial position for collision detection
            previousPosition = transform.position;

            Debug.Log($"Initialized mortar projectile targeting: {data.TargetPosition}, ArcHeight: {data.ArcHeight}");
        }
        else
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = data.UseGravity;
                rb.velocity = data.Direction * data.Speed;
            }
        }

        if (gameObject.activeSelf)
            StartCoroutine(LifetimeCounter());
    }
    private IEnumerator LifetimeCounter()
    {
        while (elapsedTime < data.Lifetime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        //Debug.Log($"Returning to pool {name}");
        StopAllCoroutines();
        if (pool != null)
        {
            pool.Return(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Projectile " + gameObject.name + " Hit: " + collision.gameObject.name);

        // Check if the collision object's layer is in our hitLayers mask
        if (hitLayers == (hitLayers | (1 << collision.gameObject.layer)))
        {
            switch (type)
            {
                case ProjectileType.Standard:
                    HandleStandardAttack(collision);
                    break;
                case ProjectileType.Explosive:
                    HandleExplosiveAttack();
                    break;
                case ProjectileType.DOT:
                    HandleDOTAttack();
                    break;
                case ProjectileType.Corrupted:
                    HandleCorruptionAttack(collision);
                    break;
                case ProjectileType.Spawner:
                    HandleSpawnerBehavior();
                    break;
            }
        }

        HandleCollision(collision);
    }
    protected void ApplyDamage(Damageable target)
    {
        target.TakeDamage(damage);
    }
    protected void HandleCollision(Collision collision)
    {
        if (audioPlayer != null && audioPlayer.isActiveAndEnabled)
            audioPlayer.PlayOneShot(bulletSound);
        if (vfxPrefab != null)
        {// if you have a VFX assigned, play it on collision

            // Get VFX from pool
            if (!PoolManager.Instance.vfxPool.TryGetValue(vfxPrefab, out VFXPool pool))
            {
                Debug.LogError($"No pool found for enemy prefab: {vfxPrefab.name}");
                //return;
            }
            else
            {
                VFXPoolingBehavior vfx = pool.Get(transform.position, transform.rotation);
                vfx.Initialize(pool);
            }
        }
        if (data.CanBounce && bounceCount < data.MaxBounces)
        {
            Bounce(collision.collider);
        }
        else
        {
            ReturnToPool();
        }
    }
    protected void Bounce(Collider surface)
    {
        if (rb != null && !targetedDirection)
        {
            Vector3 reflection = Vector3.Reflect(rb.velocity, surface.transform.up);
            rb.velocity = reflection;
            damage *= data.BounceDamageMultiplier;
            bounceCount++;
        }
        else if (targetedDirection)
        {
            // Handle bouncing for arc trajectories
            Vector3 currentDirection = (data.TargetPosition - initialPosition).normalized;
            Vector3 reflection = Vector3.Reflect(currentDirection, surface.transform.up);

            // Update target position based on reflection
            float remainingDistance = arcDistance * (1 - arcProgress);
            data.TargetPosition = transform.position + (reflection * remainingDistance);

            // Reset arc parameters for the bounce
            initialPosition = transform.position;
            arcDistance = Vector3.Distance(initialPosition, data.TargetPosition);
            arcProgress = 0;

            damage *= data.BounceDamageMultiplier;
            bounceCount++;
        }
    }
    private void HandleStandardAttack(Collision collision)
    {
        if (collision.collider.TryGetComponent<Damageable>(out var damageable))
        {
            ApplyDamage(damageable);
        }
    }
    private void HandleExplosiveAttack()
    {
        // Create explosion effect if prefab is assigned
        if (PoolManager.Instance != null && explosionEffectPrefab != null)
        {
            if (!PoolManager.Instance.vfxPool.TryGetValue(explosionEffectPrefab, out VFXPool pool))
            {
                Debug.LogError($"No pool found for enemy prefab: {explosionEffectPrefab.name}");
                return;
            }
            VFXPoolingBehavior explosiveVfx = pool.Get(transform.position, transform.rotation);
            explosiveVfx.Initialize(pool);
        }
        else
        {
            Instantiate(explosionEffectPrefab, transform.position, transform.rotation);
        }

        // Use OverlapSphere with the layer mask
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, hitLayers);
        foreach (var hit in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            float damageMultiplier = damageFalloff.Evaluate(distance / explosionRadius);

            if (hit.TryGetComponent<Damageable>(out var damageable))
            {
                damageable.TakeDamage(damage * damageMultiplier);
            }
        }
    }
    private void HandleDOTAttack()
    {
        if (dotZonePrefab != null && createsDOTZone)
        {
            GameObject zoneObject = Instantiate(dotZonePrefab, transform.position, Quaternion.identity);

            if (zoneObject.TryGetComponent<DamageOverTimeZone>(out var dotZone))
            {
                // Pass the hitLayers instead of hitTag
                dotZone.Initialize(dOTDuration, dOTTickRate, dOTDamagePerTick, dOTCorruptionPerTick, dOTRadius, hitLayers);
            }
        }

        ReturnToPool();
    }
    private void HandleCorruptionAttack(Collision collision)
    {
        if (collision.transform.TryGetComponent<Damageable>(out var damageable))
        {
            if (damageable.canHoldCorruption)
            {
                damageable.IncreaseCorruption(corruptionAmount);
            }
            damageable.TakeDamage(damage);
        }
    }
    private void HandleSpawnerBehavior()
    {
        Debug.Log("Spawn Egg Spawning Enemy");
        int index = Random.Range(0, entitiesToSpawn.Length);

        GameManager.Instance.waveManager.SpawnEnemy(entitiesToSpawn[index], transform.position, true);
    }
}
