using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilyStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private LilyRepairShop lilyShop;
    public void Initialize(StructureLevels levels, int level)
    {
        if (levels is LilyLevels shermanLevels)
        {
            UpdateLilyStats(shermanLevels, level);
        }
    }

    public void UpdateStats(StructureLevels levels, int level)
    {
        if (levels is LilyLevels shermanLevels)
        {
            UpdateLilyStats(shermanLevels, level);
        }
    }

    private void UpdateLilyStats(LilyLevels LilyStats, int levelIndex)
    {
        var levelData = LilyStats.levels[levelIndex];
        foreach (var lily in lilyShop.lilyBots)
        {
            lilyShop.maxActiveLilies = levelData.maxActiveLilies;
            
            lily.patrolRange = levelData.patrolRange;
            lily.moveSpeed = levelData.moveSpeed;
            lily.repairRate = levelData.repairRate;
        }
    }
}
