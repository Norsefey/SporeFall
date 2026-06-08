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
                StructureLevel currentLevel = structure.GetCurrentLevel();
                
                if(currentLevel == null)
                {
                    Debug.LogWarning($"Structure {structure.GetStructureName()} does not have a valid current level. Skipping.");
                    continue;
                }

                Debug.Log($"Adding structure type {structure.GetStructureType()} with level {currentLevel.level} to stats dictionary.");
                
                structureStatsDict[structure.GetStructureType()] = currentLevel;

                Debug.Log($"Current stats for {structure.GetStructureType()}: Health={currentLevel.maxHealth}, PlacementCost={currentLevel.placementCost}, EnergyCost={currentLevel.energyCost}, UpgradeCost={currentLevel.upgradeCost}");
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
        if (structureStatsDict.TryGetValue(type, out StructureLevel currentLevel))
        {
            structureStatsDict[type] = newLevel;
            Debug.Log($"Upgraded structure type {type} to level {structureStatsDict[type].level}. New stats: Health={structureStatsDict[type].maxHealth}, PlacementCost={structureStatsDict[type].placementCost}, EnergyCost={structureStatsDict[type].energyCost}, UpgradeCost={structureStatsDict[type].upgradeCost}");
        }
        else
        {
            Debug.LogWarning($"Cannot upgrade structure type {type} because it was not found in stats dictionary.");
        }
    }
}
