using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private StructureHP wallHP;
    public Wall wall;
    public void Initialize(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is WallLevels wallLevels)
        {
            UpdateWallStats(wallLevels, level, waveMultiplier);
        }
    }

    public void UpdateStats(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is WallLevels wallLevels)
        {
            UpdateWallStats(wallLevels, level, waveMultiplier);
        }
    }

    private void UpdateWallStats(WallLevels levels, int level, float waveMultiplier)
    {
        var levelData = levels.levels[level];

        wallHP.SetMaxHP(levelData.maxHealth * waveMultiplier);
        if(wall != null)
        {
            wall.protectionRange = levelData.protectionRange;
            wall.damageReduction = levelData.damageReduction;
        }

    }
}
