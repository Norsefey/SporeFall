using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrowerStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private FlameThrower flamey;

    public void Initialize(StructureLevels levels, int level)
    {
        if (levels is FlameThrowerLevels flameThrowerLevels)
        {
            UpdateFlameyStats(flameThrowerLevels, level);
        }
    }

    public void UpdateStats(StructureLevels levels, int level)
    {
        if (levels is FlameThrowerLevels flameThrowerLevels)
        {
            UpdateFlameyStats(flameThrowerLevels, level);
        }
    }

    private void UpdateFlameyStats(FlameThrowerLevels levels, int level)
    {
        var levelData = levels.levels[level];

        flamey.damagePerSecond = levelData.damagePerSecond;
        flamey.range = levelData.range;
        flamey.tickRate = levelData.tickRate;

    }
}
