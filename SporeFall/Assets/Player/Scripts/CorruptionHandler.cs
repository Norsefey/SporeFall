using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionHandler : MonoBehaviour
{
    private PlayerManager pMan;
    [SerializeField] private GameObject corruptedRobot;
    [Header("Corruption Variables")]
    [SerializeField] private float maxCorruption = 100;
    public float MaxCorruption => maxCorruption;
    public float corruptionAmount = 0;

    [Header("Large Reduction Purchase")]
    public float corruptionReductionAmount = 25f; // Amount reduced per purchase
    public int corruptionReductionCost = 100; // Cost to reduce corruption
    public float timer = 0;

    [Header("Corruption Recovery")]
    [SerializeField] private float corruptionRecoveryDelay = 3f; // Time to wait before corruption starts decreasing
    [SerializeField] private float corruptionDecreaseRate = 5f; // Units per second

    [Header("Corruption Stages")]
    [SerializeField] private float[] corruptionThresholds = new float[] { 30f, 60f, 90f }; // Thresholds for different vision effects
    private int currentCorruptionStage = 0;

    public bool preventFullCorruption = false;

    private void Start()
    {
        pMan.pUI.UpdateCorruptionDisplay(corruptionAmount);
        UpdateCorruptionVision();
    }
    // Update is called once per frame
    void Update()
    {
        if (corruptionAmount >= maxCorruption && !preventFullCorruption)
        {
            CorruptPlayer();
            return;
        }

        if (pMan.holdingCorruption && pMan.pHealth.canHoldCorruption)
        {
            // Reset timer when holding corruption
            timer = corruptionRecoveryDelay;
            corruptionAmount = Mathf.Min(corruptionAmount + pMan.currentWeapon.corruptionRate * Time.deltaTime, maxCorruption);
            UpdateCorruptionVision();
            pMan.pUI.UpdateCorruptionDisplay(corruptionAmount);
        }
        else if (corruptionAmount > 0 && timer <= 0)
        {
            // Decrease corruption level over time
            corruptionAmount = Mathf.Max(0, corruptionAmount - corruptionDecreaseRate * Time.deltaTime);
            UpdateCorruptionVision();
            pMan.pUI.UpdateCorruptionDisplay(corruptionAmount);
        }
        else if(corruptionAmount > 0)
        {
            timer -= Time.deltaTime;
        }

    }
    public void IncreaseCorruption(float value)
    {
        float previousCorruption = corruptionAmount;
        // Reset timer when corrupted
        timer = corruptionRecoveryDelay;
        corruptionAmount += value;

        if (pMan != null && pMan.audioSource != null)
        {
            float threshold = 0.75f * maxCorruption;
           if (previousCorruption < threshold && corruptionAmount >= threshold)
                pMan.audioSource.PlayOneShot(pMan.corruption75Sound, 1.5f);
        }

        pMan.pUI.UpdateCorruptionDisplay(corruptionAmount);
    }
    private void UpdateCorruptionVision()
    {
        float corruptionPercentage = (corruptionAmount / maxCorruption) * 100;
        Debug.Log(corruptionPercentage + "% Corruption");
        
        if(corruptionPercentage >= corruptionThresholds[0])
        {
            for (int i = 0; i < corruptionThresholds.Length; i++)
            {
                if (corruptionPercentage >= corruptionThresholds[i] && currentCorruptionStage < i + 1)
                {
                    if (i + 1 != currentCorruptionStage)
                    {
                        pMan.pUI.UpdateCorruptedVision(i + 1);
                        currentCorruptionStage = i + 1;
                        break;
                    }
                }
            }
        }
        else if(currentCorruptionStage != 0)
        {
            currentCorruptionStage = 0;
            pMan.pUI.UpdateCorruptedVision(currentCorruptionStage);

        }
        /* // Calculate current corruption stage based on thresholds
        int newStage = 0;
        for (int i = 0; i < corruptionThresholds.Length; i++)
        {
            if (corruptionAmount / maxCorruption >= corruptionThresholds[i])
            {
                newStage = i + 1;
            }
        }

        // Only update UI if stage has changed
        if (newStage != currentCorruptionStage)
        {
            currentCorruptionStage = newStage;
            pMan.pUI.UpdateCorruptedVision(currentCorruptionStage);
        }*/
    }
    public bool TryPurchaseCorruptionReduction()
    {
        // Check if player has enough currency (you'll need to implement this check)
        if (GameManager.Instance.Mycelia >= corruptionReductionCost)
        {
            if(corruptionAmount > 0)
            {
                return false;
            }
            // Deduct currency
            GameManager.Instance.DecreaseMycelia(corruptionReductionCost);

            // Reduce corruption
            corruptionAmount = Mathf.Max(0, corruptionAmount - corruptionReductionAmount);

            // Update visuals
            UpdateCorruptionVision();
            pMan.pUI.UpdateCorruptionDisplay(corruptionAmount);

            return true;
        }
        return false;
    }
    public void SetMaxCorruption(float value)
    {
        maxCorruption = value;
        pMan.pUI.UpdateCorruptionDisplay(corruptionAmount);
    }
    private void CorruptPlayer()
    {
        if (pMan.audioSource != null)
        {
            pMan.audioSource.Stop();
            pMan.audioSource.PlayOneShot(pMan.corruption100Sound, 1.5f);
        }
        // player loses life and respawns
        pMan.pHealth.isDead = true;
        pMan.pHealth.DepleteLife();
        pMan.TogglePControl(false);
        pMan.pAnime.ToggleIKAim(false);
        pMan.pAnime.ActivateATrigger("Corrupted");
        pMan.StartRespawn(3, true);

        corruptionAmount = 0;
        pMan.pUI.UpdateCorruptionDisplay(corruptionAmount);
        // Reset Corruption Vision
        pMan.pUI.UpdateCorruptedVision(0);
        // add a delay to corrupted robot spawning
        Invoke(nameof(SpawnCorruptedRobot), 2);
    }
    private void SpawnCorruptedRobot()
    {
        // Spawn a corrupted Player
        CorruptedPlayer robert = Instantiate(corruptedRobot, pMan.pController.transform.position, Quaternion.identity).GetComponent<CorruptedPlayer>();
        // corrupted player will prioritize attacking the player
        robert.myPlayer = pMan;
        if(GameManager.Instance.trainHandler != null)
        {
            robert.AssignDefaultTargets(GameManager.Instance.trainHandler, pMan.pController.transform);
            robert.transform.SetParent(GameManager.Instance.trainHandler.dropsHolder, true);
            GameManager.Instance.waveManager.AddRobert(robert.gameObject);
        }
        else
        {
            robert.SetTarget(pMan.pController.transform);
        }
    }
    public void ResetCorruptionLevel()
    {
        corruptionAmount = 0;
        pMan.pUI.UpdateCorruptionDisplay(0);
        pMan.pUI.UpdateCorruptedVision(0);
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
