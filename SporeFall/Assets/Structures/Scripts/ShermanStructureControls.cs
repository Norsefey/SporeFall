using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShermanStructureControls : MonoBehaviour
{
    public GameObject shermanPrefab;        // The prefab to spawn
    public Transform spawnPoint;     // Where to spawn the prefab
    private GameObject currentSherman; // Holds reference to the current spawned object
    public float respawnDelay = 2f;  // Delay time in seconds before respawning

    private bool isSpawning = false; // To prevent multiple coroutines
    [SerializeField] private SkinnedMeshRenderer sHouse;
    private float elapsedTime = 0f;
    float blendShapeValue = 0;
    void Start()
    {
        StartCoroutine(SpawnAfterDelay());
    }

    void Update()
    {
        // If there is no active prefab and we're not already spawning one
        if (currentSherman == null && !isSpawning)
        {
            StartCoroutine(SpawnAfterDelay());
        }
        else if (!isSpawning && blendShapeValue > 0)
        {
            CloseHatch();
        }
        else if (isSpawning)
        {
            OpenHatch();
        }
    }

    // Coroutine to handle the delay before spawning
    private IEnumerator SpawnAfterDelay()
    {
        elapsedTime = 0;
        isSpawning = true; // Prevent additional coroutines from starting
                       
        yield return new WaitForSeconds(respawnDelay); // Wait for the delay
        SpawnPrefab();

        elapsedTime = 0f;
        isSpawning = false; // Allow spawning again
    }

    private void OpenHatch()
    {
        elapsedTime += Time.deltaTime;  // Accumulate elapsed time
                                        // Lerp between the start and target blend shape value
        blendShapeValue = Mathf.Lerp(0, 100, elapsedTime / respawnDelay);
        // Apply the blend shape value
        sHouse.SetBlendShapeWeight(0, blendShapeValue);
    }
    private void CloseHatch()
    {
        elapsedTime += Time.deltaTime;
        blendShapeValue = Mathf.Lerp(100, 0, elapsedTime / respawnDelay);
        // Apply the blend shape value
        sHouse.SetBlendShapeWeight(0, blendShapeValue);
    }

    // Method to spawn the prefab at the spawn point
    private void SpawnPrefab()
    {
        if (shermanPrefab != null && spawnPoint != null)
        {
            currentSherman = Instantiate(shermanPrefab, spawnPoint);
            currentSherman.transform.position = spawnPoint.position;
        }
    }
}