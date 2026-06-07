using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private StructureHP wallHP;
    public Wall wall;
    
    private WallLevel currentLevel;
    public void Initialize(StructureLevel level)
    {
        if (level is WallLevel wallLevel)
        {
            UpdateWallStats(wallLevel);
        }
    }

    public void UpdateStats(StructureLevel newLevel)
    {
        if (newLevel is WallLevel wallLevel)
        {
            UpdateWallStats(wallLevel);
        }
    }

    private void UpdateWallStats(WallLevel levelData)
    {
        currentLevel = levelData;

        wallHP.SetMaxHP(levelData.maxHealth);
        
        if(wall != null)
        {
            wall.protectionRange = levelData.protectionRange;
            wall.damageReduction = levelData.damageReduction;
        }

    }
}
