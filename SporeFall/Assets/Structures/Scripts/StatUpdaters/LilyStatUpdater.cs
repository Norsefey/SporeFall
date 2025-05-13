using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilyStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private LilyRepairShop lilyShop;
    public void Initialize(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is LilyLevels shermanLevels)
        {
            UpdateLilyStats(shermanLevels, level, waveMultiplier);
        }
    }

    public void UpdateStats(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is LilyLevels shermanLevels)
        {
            UpdateLilyStats(shermanLevels, level, waveMultiplier);
        }
    }

    private void UpdateLilyStats(LilyLevels LilyStats, int levelIndex, float waveMultiplier)
    {
        var levelData = LilyStats.levels[levelIndex];
        lilyShop.ReturnAllBots();
        lilyShop.maxActiveLilies = levelData.maxActiveLilies;

        foreach (var lily in lilyShop.lilyBots)
        {
            lily.patrolRange = levelData.patrolRange;
            lily.moveSpeed = levelData.moveSpeed;
            lily.repairRate = levelData.repairRate * waveMultiplier;
        }

        if(gameObject.activeSelf)
            StartCoroutine(lilyShop.ActivateLilyBots());

    }
}
