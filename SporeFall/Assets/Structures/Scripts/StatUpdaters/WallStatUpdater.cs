using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private StructureHP wallHP;
    public void Initialize(StructureLevels levels, int level)
    {
        if (levels is WallLevels wallLevels)
        {
            UpdateWallStats(wallLevels, level);
        }
    }

    public void UpdateStats(StructureLevels levels, int level)
    {
        if (levels is WallLevels wallLevels)
        {
            UpdateWallStats(wallLevels, level);
        }
    }

    private void UpdateWallStats(WallLevels levels, int level)
    {
        var levelData = levels.levels[level];

        wallHP.SetMaxHP(levelData.maxHealth);
    }
}
