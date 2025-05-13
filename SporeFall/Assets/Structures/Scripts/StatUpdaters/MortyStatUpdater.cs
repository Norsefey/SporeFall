using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MortyStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private MortyControlScript morty;

    public void Initialize(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is MortyLevels turretLevels)
        {
            UpdateMortyStats(turretLevels, level, waveMultiplier);
        }
    }

    public void UpdateStats(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is MortyLevels turretLevels)
        {
            UpdateMortyStats(turretLevels, level, waveMultiplier);
        }
    }
    private void UpdateMortyStats(MortyLevels levels, int level, float waveMultiplier)
    {
        var levelData = levels.levels[level];
        morty.detectionRange = levelData.detectionRange;
        morty.fireRate = levelData.fireRate;
        morty.fireRange = levelData.fireRange;
        // Set up bullet data
        morty.bulletData = levelData.projectileStats;
        morty.bulletData.Damage = levelData.projectileStats.Damage * waveMultiplier;
    }
}
