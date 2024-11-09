using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionHandler : MonoBehaviour
{
    private PlayerManager pMan;
    [SerializeField] private GameObject corruptedRobot;
    [Header("Corruption Variables")]
    public float maxCorruption = 100;
    public float corruptionLevel = 0;
    public float corruptionReductionAmount = 25f; // Amount reduced per purchase
    public int corruptionReductionCost = 100; // Cost to reduce corruption


    [Header("Corruption Stages")]
    [SerializeField] private float[] corruptionThresholds = new float[] { 30f, 60f, 90f }; // Thresholds for different vision effects
    private int currentCorruptionStage = 0;

    private void Start()
    {
        UpdateCorruptionVision();
    }
    // Update is called once per frame
    void Update()
    {
        if (corruptionLevel >= maxCorruption)
        {
            CorruptPlayer();
            return;
        }

        if (pMan.holdingCorruption)
        {
            corruptionLevel = Mathf.Min(corruptionLevel + pMan.currentWeapon.corruptionRate * Time.deltaTime, maxCorruption);
            UpdateCorruptionVision();
            pMan.pUI.UpdateCorruptionDisplay(corruptionLevel);
        }

    }
    private void UpdateCorruptionVision()
    {
        // Calculate current corruption stage based on thresholds
        int newStage = 0;
        for (int i = 0; i < corruptionThresholds.Length; i++)
        {
            if (corruptionLevel >= corruptionThresholds[i])
            {
                newStage = i + 1;
            }
        }

        // Only update UI if stage has changed
        if (newStage != currentCorruptionStage)
        {
            currentCorruptionStage = newStage;
            pMan.pUI.UpdateCorruptedVision(currentCorruptionStage);
        }
    }
    public bool TryPurchaseCorruptionReduction()
    {
        // Check if player has enough currency (you'll need to implement this check)
        if (pMan.mycelia >= corruptionReductionCost)
        {
            if(corruptionLevel > 0)
            {
                return false;
            }
            // Deduct currency
            pMan.mycelia -= corruptionReductionCost;

            // Reduce corruption
            corruptionLevel = Mathf.Max(0, corruptionLevel - corruptionReductionAmount);

            // Update visuals
            UpdateCorruptionVision();
            pMan.pUI.UpdateCorruptionDisplay(corruptionLevel);

            return true;
        }
        return false;
    }
    private void CorruptPlayer()
    {
        corruptionLevel = 0;
        pMan.pUI.UpdateCorruptionDisplay(corruptionLevel);
        // Reset Corruption Vision
        pMan.pUI.UpdateCorruptedVision(0);
        // add a delay to coruppted robot spawning
        Invoke(nameof(SpawnCorruptedRobot), 1);
        // player loses life and respawns
        pMan.pHealth.DepleteLife();
        pMan.StartRespawn();
    }
    private void SpawnCorruptedRobot()
    {
        // Spawn a corrupted Player
        CorruptedPlayer robert = Instantiate(corruptedRobot, pMan.pController.transform.position, Quaternion.identity).GetComponent<CorruptedPlayer>();
        // corrupted player will prioritize attacking the player
        robert.myPlayer = pMan;
        robert.AssignDefaultTarget(pMan.train, pMan.pController.transform);
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
