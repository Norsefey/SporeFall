using System.Collections;
using UnityEngine;

public class RepairStationStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private RepairController repairStation;
    private RepairLevel currentLevel;
    public void Initialize(StructureLevel level)
    {
        if (level is RepairLevel repairLevel)
        {
            UpdateRepairStationStats(repairLevel);
        }
    }

    public void UpdateStats(StructureLevel newLevel)
    {
        if (newLevel is RepairLevel repairLevel)
        {
            UpdateRepairStationStats(repairLevel);
        }
    }

    private void UpdateRepairStationStats(RepairLevel levelData)
    {
        currentLevel = levelData;

        repairStation.healAmount = levelData.healAmount;
        repairStation.healRate = levelData.healRate;
        repairStation.healRadius = levelData.healRange;

        float scale = levelData.healRange;
        repairStation.transform.localScale = new Vector3(scale, scale, scale);
    }
}
