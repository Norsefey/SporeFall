using System.Collections;
using UnityEngine;

public class RepairStationStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private RepairController repairStation;
    public void Initialize(StructureLevels levels, int level)
    {
        if (levels is RepairLevels repairStation)
        {
            UpdateRepairStationStats(repairStation, level);
        }
    }

    public void UpdateStats(StructureLevels levels, int level)
    {
        if (levels is RepairLevels repairStation)
        {
            UpdateRepairStationStats(repairStation, level);
        }
    }

    private void UpdateRepairStationStats(RepairLevels levels, int level)
    {
        // get the data of the correct level of the structure
        var levelData = levels.levels[level];

        repairStation.healAmount = levelData.healAmount;
        repairStation.healRate = levelData.healRate;
        repairStation.healRadius = levelData.healRange;

        float scale = levelData.healRange;
        repairStation.transform.localScale = new Vector3(scale, scale, scale);
    }
}
