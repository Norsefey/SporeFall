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
        UpdateCorruptionVision();
    }
    // Update is called once per frame
    void Update()
    {
        if (corruptionLevel >= maxCorruption && !preventFullCorruption)
        {
            CorruptPlayer();
            return;
        }

        if (pMan.holdingCorruption && pMan.pHealth.canHoldCorruption)
        {
            // Reset timer when holding corruption
            timer = corruptionRecoveryDelay;
            corruptionLevel = Mathf.Min(corruptionLevel + pMan.currentWeapon.corruptionRate * Time.deltaTime, maxCorruption);
            UpdateCorruptionVision();
            pMan.pUI.UpdateCorruptionDisplay(corruptionLevel);
        }
        else if (corruptionLevel > 0 && timer <= 0)
        {
            // Decrease corruption level over time
            corruptionLevel = Mathf.Max(0, corruptionLevel - corruptionDecreaseRate * Time.deltaTime);
            UpdateCorruptionVision();
            pMan.pUI.UpdateCorruptionDisplay(corruptionLevel);
        }
        else if(corruptionLevel > 0)
        {
            timer -= Time.deltaTime;
        }

    }
    public void IncreaseCorruption(float value)
    {
        float previousCorruption = corruptionLevel;
        // Reset timer when corrupted
        timer = corruptionRecoveryDelay;
        corruptionLevel += value;

        if (pMan != null && pMan.audioSource != null)
        {
           /* if (previousCorruption < 25 && corruptionLevel >= 25)
                pMan.audioSource.PlayOneShot(pMan.corruption25Sound,1.5f);
            else if (previousCorruption < 50 && corruptionLevel >= 50)
                pMan.audioSource.PlayOneShot(pMan.corruption50Sound, 1.5f);
            else */if (previousCorruption < 75 && corruptionLevel >= 75)
                pMan.audioSource.PlayOneShot(pMan.corruption75Sound, 1.5f);
          /*  else if (previousCorruption < 100 && corruptionLevel >= 100)
                pMan.audioSource.PlayOneShot(pMan.corruption100Sound, 1.5f);*/
        }

        pMan.pUI.UpdateCorruptionDisplay(corruptionLevel);
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
        if (GameManager.Instance.Mycelia >= corruptionReductionCost)
        {
            if(corruptionLevel > 0)
            {
                return false;
            }
            // Deduct currency
            GameManager.Instance.DecreaseMycelia(corruptionReductionCost);

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
        if (pMan.audioSource != null)
        {
            pMan.audioSource.Stop();
            pMan.audioSource.PlayOneShot(pMan.corruption100Sound, 1.5f);
        }
        // player loses life and respawns
        pMan.pHealth.DepleteLife();
        pMan.TogglePControl(false);
        pMan.pAnime.ToggleIKAim(false);
        pMan.pAnime.ActivateATrigger("Corrupted");
        pMan.StartRespawn(3, true);

        corruptionLevel = 0;
        pMan.pUI.UpdateCorruptionDisplay(corruptionLevel);
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
        robert.AssignDefaultTarget(GameManager.Instance.trainHandler, pMan.pController.transform);
        robert.transform.SetParent(GameManager.Instance.trainHandler.dropsHolder, true);

        GameManager.Instance.waveManager.AddRobert(robert.gameObject);
    }
    public void ResetCorruptionLevel()
    {
        corruptionLevel = 0;
        pMan.pUI.UpdateCorruptionDisplay(0);
        pMan.pUI.UpdateCorruptedVision(0);
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
