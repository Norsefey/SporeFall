using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShermanStructureControls : MonoBehaviour
{
    public GameObject prefab;        // The prefab to spawn
    public Transform spawnPoint;     // Where to spawn the prefab
    private GameObject currentSpawn; // Holds reference to the current spawned object
    public float respawnDelay = 2f;  // Delay time in seconds before respawning

    private bool isSpawning = false; // To prevent multiple coroutines

    void Start()
    {
        SpawnPrefab();
    }

    void Update()
    {
        // If there is no active prefab and we're not already spawning one
        if (currentSpawn == null && !isSpawning)
        {
            StartCoroutine(SpawnAfterDelay());
        }
    }

    // Coroutine to handle the delay before spawning
    private IEnumerator SpawnAfterDelay()
    {
        isSpawning = true; // Prevent additional coroutines from starting
        yield return new WaitForSeconds(respawnDelay); // Wait for the delay
        SpawnPrefab();
        isSpawning = false; // Allow spawning again
    }

    // Method to spawn the prefab at the spawn point
    private void SpawnPrefab()
    {
        if (prefab != null && spawnPoint != null)
        {
            currentSpawn = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}