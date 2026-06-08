using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilyStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private LilyRepairShop lilyShop;

    private LilyLevel currentLevel;

    public void Initialize(StructureLevel level)
    {
        if (level is LilyLevel lilyLevel)
        {
            UpdateLilyStats(lilyLevel);
        }
    }

    public void UpdateStats(StructureLevel newLevel)
    {
        if (newLevel is LilyLevel lilyLevel)
        {
            UpdateLilyStats(lilyLevel);
        }
    }

    private void UpdateLilyStats(LilyLevel levelData)
    {
        currentLevel = levelData;
        lilyShop.ReturnAllBots();
       
        lilyShop.SpawnLilyBot(levelData.maxActiveLilies);

        foreach (var lily in lilyShop.spawnedLilyBots)
        {
            lily.patrolRange = levelData.patrolRange;
            lily.moveSpeed = levelData.moveSpeed;
            lily.repairRate = levelData.repairRate;
        }

        if (gameObject.activeSelf)
            StartCoroutine(lilyShop.ActivateLilyBots());

    }
}
