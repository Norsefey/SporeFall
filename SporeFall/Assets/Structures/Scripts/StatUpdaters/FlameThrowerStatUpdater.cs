using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrowerStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private FlameThrower flamey;

    public void Initialize(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is FlameThrowerLevels flameThrowerLevels)
        {
            UpdateFlameyStats(flameThrowerLevels, level, waveMultiplier);
        }
    }

    public void UpdateStats(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is FlameThrowerLevels flameThrowerLevels)
        {
            UpdateFlameyStats(flameThrowerLevels, level, waveMultiplier);
        }
    }

    private void UpdateFlameyStats(FlameThrowerLevels levels, int level, float waveMultiplier)
    {
        var levelData = levels.levels[level];

        flamey.damageAmount = levelData.damageAmount * (waveMultiplier / 2);
        flamey.range = levelData.range;
        flamey.damageTickRate = levelData.damageTickRate;

    }
}
