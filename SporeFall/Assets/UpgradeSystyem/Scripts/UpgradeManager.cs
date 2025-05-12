using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType
{// Structures will be upgraded across the board based on their type
    Terry,
    Frankie,
    Sherman,
    Ricardo,
    Walter,
    Lily,
    John,
    Stanley,
    Morty
}
public class UpgradeManager : MonoBehaviour
{
    public Dictionary<StructureType, int> currentStructureLevel = new Dictionary<StructureType, int>();
    private List<StructureLevels> structureStats = new List<StructureLevels>();

    private void Start()
    {
        foreach(GameObject structureObj in GameManager.Instance.availableStructures)
        {
            Structure structure = structureObj.GetComponent<Structure>();
            structureStats.Add(structure.structureStats);

            currentStructureLevel[structure.structureStats.type] = 0;
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
    public void UpgradeStructure(StructureType type)
    {
        Debug.Log("current Level: " + currentStructureLevel[type]);
        currentStructureLevel[type]++;
        Debug.Log("New Level: " + currentStructureLevel[type]);
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
