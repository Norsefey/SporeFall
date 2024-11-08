using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShermanStructureControls : MonoBehaviour
{
    public GameObject shermanPrefab;        // The prefab to spawn
    public Transform spawnPoint;     // Where to spawn the prefab
    private ShermanControl currentSherman; // Holds reference to the current spawned object
    public float respawnDelay = 2f;  // Delay time in seconds before respawning

    [SerializeField] private SkinnedMeshRenderer sHouse;
    //float blendShapeValue = 0;
    void Start()
    {
        currentSherman = Instantiate(shermanPrefab, spawnPoint).GetComponent<ShermanControl>();
        currentSherman.SetParent(this);
    }

    void Update()
    {
      /*  // If there is no active prefab and we're not already spawning one
        if (currentSherman == null && !isSpawning)
        {
            StartCoroutine(SpawnAfterDelay());
        }*/
        /*else if (!isSpawning && blendShapeValue > 0)
        {
            CloseHatch();
        }
        else if (isSpawning)
        {
            OpenHatch();
        }*/
    }
    private void OnDisable()
    {
        currentSherman.DeactivateSherman();
    }
    private void OnEnable()
    {
        StartCoroutine(ResetAfterDelay());
    }
    // Coroutine to handle the delay before spawning
    public IEnumerator ResetAfterDelay()
    {
        if(currentSherman != null)
            currentSherman.DeactivateSherman();
        yield return new WaitForSeconds(respawnDelay); // Wait for the delay
        ResetSherman();
    }

    /*private void OpenHatch()
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
    }*/

    // Method to spawn the prefab at the spawn point
    private void ResetSherman()
    {
        if (shermanPrefab != null && spawnPoint != null)
        {
            currentSherman.ActivateSherman();
            currentSherman.transform.position = spawnPoint.position;
        }
    }
}