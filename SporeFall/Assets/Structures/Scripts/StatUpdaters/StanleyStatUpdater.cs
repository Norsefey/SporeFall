using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanleyStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private StanleyControlScript stanley;
    private StanleyLevel currentLevel;

    public void Initialize(StructureLevel level)
    {
        if(level is StanleyLevel stanleyLevel)
        {
            UpdateStanleyStats(stanleyLevel);
        }
    }

    public void UpdateStats(StructureLevel newLevel)
    {
        if (newLevel is StanleyLevel stanleyLevel)
        {
            UpdateStanleyStats(stanleyLevel);
        }
    }

    private void UpdateStanleyStats(StanleyLevel levelData)
    {
        currentLevel = levelData;
        stanley.UpdateVisual(levelData.level);

        stanley.moveSpeed = levelData.moveSpeed;
        stanley.changeDirectionInterval = levelData.changeDirectionInterval;
        stanley.detectionRadius = levelData.detectionRadius;
        stanley.randomMovementWeight = levelData.randomMovementWeight;

        stanley.myceliaGenerationRate = levelData.myceliaGenerationRate;
        stanley.myceliaGenerationTickRate = levelData.myceliaGenerationTickRate;
    }
}
