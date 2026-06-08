using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType
{// Structures will be upgraded across the board based on their type
    Turret,
    Flamethrower,
    ExplosiveBot,
    HealStation,
    DefenseWall,
    RepairBot,
    Platform,
    ConverterBot,
    Mortar
}
public class UpgradeManager : MonoBehaviour
{
    public Dictionary<StructureType, StructureLevel> structureStatsDict = new Dictionary<StructureType, StructureLevel>();

    private void Start()
    {
        foreach (GameObject structureObj in GameManager.Instance.availableStructures)
        {
            Structure structure = structureObj.GetComponent<Structure>();

            if (structure != null)
            {
                StructureLevel currentLevel = structure.structureStats.GetBaseLevel();

                Debug.Log($"{currentLevel.upgradePlacementCostMultiplier}");

                structureStatsDict[structure.GetStructureType()] = currentLevel;
            }
        }
    }
    public bool CanUpgrade(StructureType type, float availableMycelia)
    {
        if (structureStatsDict.TryGetValue(type, out StructureLevel currentLevel))
        {
            if (availableMycelia >= currentLevel.upgradeCost)
            {
                return true;
            }
        }
        
        return false;
    }
    public StructureLevel GetStructureLevelOfType(StructureType type)
    {
        if (structureStatsDict.TryGetValue(type, out StructureLevel stats))
        {
            return stats;
        }
        Debug.LogWarning($"Structure type {type} not found in stats dictionary.");
        return null;
    }
    public void UpgradeStructure(StructureType type, StructureLevel newLevel)
    {
        structureStatsDict[type] = newLevel;
    }
}
