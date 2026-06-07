using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MortyStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private MortyControlScript morty;
    private MortyLevel currentLevel;
    public void Initialize(StructureLevel level)
    {
        if (level is MortyLevel mortyLevel)
        {
            UpdateMortyStats(mortyLevel);
        }
    }

    public void UpdateStats(StructureLevel newLevel)
    {
        if (newLevel is MortyLevel mortyLevel)
        {
            UpdateMortyStats(mortyLevel);
        }
    }
    private void UpdateMortyStats(MortyLevel levelData)
    {
        currentLevel = levelData;
        morty.detectionRange = levelData.detectionRange;
        morty.fireRate = levelData.fireRate;
        morty.fireRange = levelData.fireRange;
        // Set up bullet data
        morty.bulletData = levelData.projectileStats;
    }
}
