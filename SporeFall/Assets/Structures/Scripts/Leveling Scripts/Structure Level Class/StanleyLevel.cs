using System;
using UnityEngine;

[Serializable]
public class StanleyLevel : StructureLevel
{
    [Header("Base Stanley States")]
    [Tooltip("How often to give mycelia")]
    public float myceliaGenerationTickRate;
    [Tooltip("How Much mycelia to give")]
    public float myceliaGenerationRate;

    public float moveSpeed = 2f;
    public float turnSpeed = 1f;    // How fast the object changes direction
    public float changeDirectionInterval = 2f;    // How often to change direction
    public float detectionRadius = 10f;    // Detection radius for nearby Mycelia
    public float randomMovementWeight = 1f;    // Weight for random movement (higher = more random movement)

    [Header("Stanley Leveling")]
    public float upgradeMyceliaGenerationTickRateMultiplier = 1.1f;
    public float upgradeMyceliaGenerationRateMultiplier = 1.1f;


    public override StructureLevel NextLevel()
    {
        StanleyLevel nextLevel = new StanleyLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            placementCost = this.placementCost * upgradePlacementCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
            upgradeCost = this.upgradeCost * upgradeUpgradeCostMultiplier,

            moveSpeed = this.moveSpeed,
            turnSpeed = this.turnSpeed,
            changeDirectionInterval = this.changeDirectionInterval,
            detectionRadius = this.detectionRadius,
            randomMovementWeight = this.randomMovementWeight,

            myceliaGenerationTickRate = this.myceliaGenerationTickRate * upgradeMyceliaGenerationTickRateMultiplier,
            myceliaGenerationRate = this.myceliaGenerationRate * upgradeMyceliaGenerationRateMultiplier,

        };

        nextLevel.upgradeDescription =
                                $"Max Health: {maxHealth:F1} -> <color=yellow>{nextLevel.maxHealth:F1}</color>, \n" +
                                $"Mycelia Generation Tick Rate: {myceliaGenerationTickRate:F2} -> <color=yellow>{nextLevel.myceliaGenerationTickRate:F2}</color> seconds, \n" +
                                $"Mycelia Generation Rate: {myceliaGenerationRate:F1} -> <color=yellow>{nextLevel.myceliaGenerationRate:F1}</color> mycelia/tick, \n" +
                                $"Placement Cost: {placementCost:F1} -> <color=yellow>{nextLevel.placementCost:F1}</color>," +
                                $"Energy Cost: {energyCost:F1} -> <color=yellow>{nextLevel.energyCost:F1}</color>, ";
        return nextLevel;
    }
}
