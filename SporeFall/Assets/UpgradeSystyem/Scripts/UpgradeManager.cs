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
    private List<StructureStats> structureStats = new List<StructureStats>();

    private void Start()
    {
        foreach (GameObject structureObj in GameManager.Instance.availableStructures)
        {
            Structure structure = structureObj.GetComponent<Structure>();
            structureStats.Add(structure.GetStructureStats());
        }
    }
    public bool CanUpgrade(StructureType type, float availableMycelia)
    {
        StructureStats structureLevelData = GetStructureStatsForType(type);
        if (structureLevelData == null) return false;

        if (availableMycelia >= structureLevelData.currentLevel.upgradeCost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
   
    public StructureStats GetStructureStatsForType(StructureType type)
    {
        return structureStats.Find(s => s.type == type);
    }
}
