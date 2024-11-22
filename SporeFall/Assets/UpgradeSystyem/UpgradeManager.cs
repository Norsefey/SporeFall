using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType
{// Structures will be upgraded across the board based on their type
    Turret,
    FlameThower,
    Sherman,
    RepairStation,
    Wall
}
public class UpgradeManager : MonoBehaviour
{
    public Dictionary<StructureType, int> structureLevels = new Dictionary<StructureType, int>();
    public List<StructureLevels> structureStats = new List<StructureLevels>();

    private void Awake()
    {
        // Initialize all structure levels to 0
        foreach (var stat in structureStats)
        {
            structureLevels[stat.type] = 0;
        }
    }
    public bool CanUpgrade(StructureType type, float availableMycelia)
    {
        StructureLevels structureLevelData = GetStructureLevelsForType(type);
        if (structureLevelData == null) return false;

        int currentLevel = structureLevels[type];
        if (currentLevel + 1 >= structureLevelData.GetLevelCount()) return false;

        StructureLevel nextLevel = structureLevelData.GetLevel(currentLevel + 1);
        if (availableMycelia >= nextLevel.cost)
        {
            GameManager.Instance.TrainHandler.players[0].DecreaseMycelia(nextLevel.cost);
            return true;
        }
        else
        {
            return false;
        }
    }
    public void UpgradeStructure(StructureType type, float availableMycelia)
    {
        if (!CanUpgrade(type, availableMycelia))
            return;
        structureLevels[type]++;
    }
    public StructureLevel GetNextLevel(StructureType type)
    {
        int currentLevel = this.structureLevels[type];
        StructureLevels structureLevelData = GetStructureLevelsForType(type);
        
        if (structureLevelData == null || IsMaxLevel(type))
        {
            Debug.Log("No More levels");
            return null;
        }

        return structureLevelData.GetLevel(currentLevel + 1);
    }
    public StructureLevels GetStructureLevelsForType(StructureType type)
    {
        return structureStats.Find(s => s.type == type);
    }
    public int GetStructureLevel(StructureType type)
    {
        return structureLevels[type];
    }
    public bool IsMaxLevel(StructureType type)
    {
        StructureLevels structureLevelData = GetStructureLevelsForType(type);
        if (structureLevelData == null) return true;

        return structureLevelData.GetLevelCount() <= this.structureLevels[type] + 1;
    }
}
