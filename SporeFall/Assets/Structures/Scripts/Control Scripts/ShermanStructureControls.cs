using System.Collections;
using UnityEngine;

public class ShermanStructureControls : MonoBehaviour
{
    public Transform spawnPoint;     // Where to spawn the prefab
    public ShermanControl[] currentShermans; // Holds reference to the current spawned object
    public float respawnDelay = 2f;  // Delay time in seconds before respawning

    public int maxActiveShermans = 1;

    //[SerializeField] private SkinnedMeshRenderer sHouse;
    //float blendShapeValue = 0;
    void Start()
    {
        StartCoroutine(ResetShermanBots());
    }
    private void OnDisable()
    {
        foreach (var sherman in currentShermans) 
        {
            sherman.DeactivateSherman();
        }
    }
    private void OnEnable()
    {
        StartCoroutine(ResetShermanBots());
    }
    // Coroutine to handle the delay before spawning
    public IEnumerator ResetShermanBots()
    {
        for (int i = 0; i < maxActiveShermans; i++)
        {
            if (currentShermans[i] != null)
                currentShermans[i].DeactivateSherman();
            yield return new WaitForSeconds(respawnDelay); // Wait for the delay
            currentShermans[i].transform.position = spawnPoint.position;
            currentShermans[i].ActivateSherman();
        }
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
}