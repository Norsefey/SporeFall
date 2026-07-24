using System.Collections.Generic;
using UnityEngine;

public class OctomushTentacleController : MonoBehaviour
{

    [Header("Vulnerable Settings")]
    [SerializeField] private float vulnerableDuration = 8f; // Duration the boss remains vulnerable after tentacle destruction

    [Header("Tentacle Settings")]
    [SerializeField] private int tentacleLevel;
    [SerializeField] private int tentaclesToSpawnCount = 4;
    public List<EnemyController> tentaclesToSpawn = new();

    [Header("References")]
    [SerializeField] private EnemyHP enemyHP;

    [Header("Main Body Visuals")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material shieldedMaterial;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip shieldedDownSFX;

    private List<EnemyController> spawnedTentacles = new List<EnemyController>();
    private float vulnerableTimer = 5f; // Time the boss remains vulnerable after tentacle destruction
    private bool isVulnerable = false;
    private float tentacleCount = 0;
    private AudioSource audioPlayer;

    private void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();
    }

    private void Start()
    {
        EnableShield();
        SpawnTentacles();
        enemyHP.OnDied += OnDie;
        vulnerableTimer = vulnerableDuration; // Initialize the timer with the vulnerable duration
    }

    private void Update()
    {
        // Check if the boss is vulnerable and if the time in vulnerable state has elapsed
        if (isVulnerable)
        {
            vulnerableTimer -= Time.deltaTime;
            if (vulnerableTimer <= 0f)
            {
                isVulnerable = false;
                // Reset the timer for the next phase change
                vulnerableTimer = vulnerableDuration;
                SpawnTentacles();
                EnableShield();
            }
        }
    }

    private void SpawnTentacles()
    {
        for (int i = 0; i < tentaclesToSpawnCount; i++)
        {
            int index = Random.Range(0, tentaclesToSpawn.Count);

            SpawnTentacle(tentaclesToSpawn[index].gameObject);
        }
    }
    private void RemoveShield()
    {
        enemyHP.canTakeDamage = true;

        // Change material based on vulnerability state
        if (bodyRenderer != null)
        {
            Material[] materials = bodyRenderer.materials;
            Destroy(materials[1]);
            materials[1] = null;
            bodyRenderer.materials = materials;
            // Play shield down sound effect
            if (audioPlayer != null && shieldedDownSFX != null)
            {
                audioPlayer.PlayOneShot(shieldedDownSFX);
            }
        }
    }
    private void EnableShield()
    {
        enemyHP.canTakeDamage = false;

        // Change material back to shielded state
        if (bodyRenderer != null)
        {
            Material[] materials = bodyRenderer.materials;
            materials[1] = shieldedMaterial;
            bodyRenderer.materials = materials;
        }
    }
    private void SpawnTentacle(GameObject tentacleToSpawn)
    {
        Vector3 spawnPoint = GameManager.Instance.waveManager.GetSpawnPointWithinZone();

        EnemyController tentacle = GameManager.Instance.waveManager.SpawnEnemy(tentacleToSpawn, tentacleLevel, spawnPoint, true);

        spawnedTentacles.Add(tentacle);
        tentacle.OnDied += OnTentacleDeath;
        tentacleCount++;
    }
    private void OnTentacleDeath(EnemyController tentacle)
    {
        spawnedTentacles.Remove(tentacle);
        tentacleCount--;
        tentacle.OnDied -= OnTentacleDeath; // Unsubscribe from the event to prevent memory leaks
        CheckVulnerabilityState();
    }
    private void CheckVulnerabilityState()
    {
        if (tentacleCount <= 0)
        {
            isVulnerable = true;
            RemoveShield();
        }
        else
        {
            isVulnerable = false;
        }
    }
    public void OnDie(Damageable damageable)
    {
        // Clean up any null references first
        spawnedTentacles.RemoveAll(item => item == null);

        // Create a copy of the tentacles list to avoid modification during iteration
        List<EnemyController> tentaclesToDestroy = new List<EnemyController>(spawnedTentacles);

        // Destroy any remaining tentacles first
        foreach (var tentacle in tentaclesToDestroy)
        {
            if (tentacle != null)
                tentacle.KillSelf();
        }

        // Clear the original list
        spawnedTentacles.Clear();
    }
}
