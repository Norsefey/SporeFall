using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanleyStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private StanleyControlScript stanley;

    public void Initialize(StructureLevels levels, int level)
    {
        if(levels is StanleyLevels stanley)
        {
            UpdateStanleyStats(stanley, level);
        }
    }

    public void UpdateStats(StructureLevels levels, int level)
    {
        if (levels is StanleyLevels stanley)
        {
            UpdateStanleyStats(stanley, level);
        }
    }

    private void UpdateStanleyStats(StanleyLevels levels, int currentLevel)
    {
        var levelData = levels.levels[currentLevel];
        stanley.UpdateVisual(currentLevel);

        stanley.moveSpeed = levelData.moveSpeed;
        stanley.turnSpeed = levelData.turnSpeed;
        stanley.changeDirectionInterval = levelData.changeDirectionInterval;
        stanley.detectionRadius = levelData.detectionRadius;
        stanley.randomMovementWeight = levelData.randomMovementWeight;

        stanley.myceliaGenerationRate = levelData.myceliaGenerationRate;
        stanley.myceliaGenerationTickRate = levelData.myceliaGenerationTickRate;
    }
}
