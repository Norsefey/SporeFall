using System.Collections;
using UnityEngine;

public class RepairStationStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private RepairController repairStation;
    public void Initialize(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is RepairLevels repairStation)
        {
            UpdateRepairStationStats(repairStation, level, waveMultiplier);
        }
    }

    public void UpdateStats(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is RepairLevels repairStation)
        {
            UpdateRepairStationStats(repairStation, level, waveMultiplier);
        }
    }

    private void UpdateRepairStationStats(RepairLevels levels, int level, float waveMultiplier)
    {
        // get the data of the correct level of the structure
        var levelData = levels.levels[level];

        repairStation.healAmount = levelData.healAmount * (waveMultiplier * 2);
        repairStation.healRate = levelData.healRate;
        repairStation.healRadius = levelData.healRange;

        float scale = levelData.healRange;
        repairStation.transform.localScale = new Vector3(scale, scale, scale);
    }
}
