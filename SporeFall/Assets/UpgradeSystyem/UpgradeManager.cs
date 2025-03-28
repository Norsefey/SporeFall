using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType
{// Structures will be upgraded across the board based on their type
    Turret,
    FlameThower,
    Sherman,
    RepairStation,
    Wall,
    Lily,
    Platform
}
public class UpgradeManager : MonoBehaviour
{
    public Dictionary<StructureType, int> currentStructureLevel = new Dictionary<StructureType, int>();
    public List<StructureLevels> structureStats = new List<StructureLevels>();

    private void Awake()
    {
        // Initialize all structure levels to 0
        foreach (var stat in structureStats)
        {
            currentStructureLevel[stat.type] = 0;
        }
    }
    public bool CanUpgrade(StructureType type, float availableMycelia)
    {
        StructureLevels structureLevelData = GetStructureLevelsForType(type);
        if (structureLevelData == null) return false;

        int currentLevel = currentStructureLevel[type];
        if (currentLevel + 1 >= structureLevelData.GetLevelCount()) return false;

        StructureLevel nextLevel = structureLevelData.GetLevel(currentLevel + 1);
        if (availableMycelia >= nextLevel.cost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void UpgradeStructure(StructureType type, float availableMycelia)
    {
        currentStructureLevel[type]++;
    }
    public StructureLevel GetCurrentLevel(StructureType type)
    {
        if (!currentStructureLevel.TryGetValue(type, out int currentLevel)) return null;

        StructureLevels structureLevelData = GetStructureLevelsForType(type);
        if (structureLevelData == null || IsMaxLevel(type)) return null;

        return structureLevelData.GetLevel(currentLevel);
    }
    public StructureLevel GetNextLevel(StructureType type)
    {
        if (!currentStructureLevel.TryGetValue(type, out int currentLevel)) return null;

        StructureLevels structureLevelData = GetStructureLevelsForType(type);
        if (structureLevelData == null || IsMaxLevel(type)) return null;

        return structureLevelData.GetLevel(currentLevel + 1);
    }
    public StructureLevels GetStructureLevelsForType(StructureType type)
    {
        return structureStats.Find(s => s.type == type);
    }
    public int GetStructureLevel(StructureType type)
    {
        return currentStructureLevel.TryGetValue(type, out int level) ? level : 0;
    }
    public bool IsMaxLevel(StructureType type)
    {
        return GetStructureLevelsForType(type)?.GetLevelCount() - 1 <= GetStructureLevel(type);
    }
}
