using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanleyStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private StanleyControlScript stanley;

    public void Initialize(StructureLevels levels, int level, float waveMultiplier)
    {
        if(levels is StanleyLevels stanley)
        {
            UpdateStanleyStats(stanley, level, waveMultiplier);
        }
    }

    public void UpdateStats(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is StanleyLevels stanley)
        {
            UpdateStanleyStats(stanley, level, waveMultiplier);
        }
    }

    private void UpdateStanleyStats(StanleyLevels levels, int currentLevel, float waveMultiplier)
    {
        var levelData = levels.levels[currentLevel];
        stanley.UpdateVisual(currentLevel);

        stanley.moveSpeed = levelData.moveSpeed;
        stanley.changeDirectionInterval = levelData.changeDirectionInterval;
        stanley.detectionRadius = levelData.detectionRadius;
        stanley.randomMovementWeight = levelData.randomMovementWeight;

        stanley.myceliaGenerationRate = levelData.myceliaGenerationRate * (waveMultiplier /2);
        stanley.myceliaGenerationTickRate = levelData.myceliaGenerationTickRate;
    }
}
