using System;
using UnityEngine;

[Serializable]
public class LilyLevel : StructureLevel
{
    [Header("Base Lily Stats")]
    public int maxActiveLilies = 1;
    public float patrolRange = 7.5f;
    public float repairRate = 7;
    public float moveSpeed = 2f;

    [Header("Lily Leveling")]
    public int upgradeMaxActiveLilies = 99; // Max limit for active lilies
    public float upgradePatrolRangeMultiplier = 1.1f; // 10% increase in patrol range per level
    public float upgradeRepairRateMultiplier = 1.2f; // 20% increase in repair rate per level
    public float upgradeMoveSpeedMultiplier = 1.1f; // 10% increase in move speed per level

    public override StructureLevel NextLevel()
    {
        LilyLevel nextLevel = new LilyLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            placementCost = this.placementCost * upgradePlacementCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
            upgradeCost = this.upgradeCost * upgradeUpgradeCostMultiplier,

            maxActiveLilies = (int)MathF.Min(this.maxActiveLilies + 1, upgradeMaxActiveLilies), // Increase max active lilies by 1 each level
            patrolRange = this.patrolRange * upgradePatrolRangeMultiplier, // Increase patrol range by multiplier
            repairRate = this.repairRate * upgradeRepairRateMultiplier, // Increase repair rate by multiplier
            moveSpeed = this.moveSpeed * upgradeMoveSpeedMultiplier // Increase move speed by multiplier
        };

        nextLevel.upgradeDescription =
                                $"Health: {maxHealth:F1}  -> <color=yellow> {nextLevel.maxHealth:F1} </color>, " +
                                $"Max Active Lilies: {maxActiveLilies} -> <color=yellow>{nextLevel.maxActiveLilies}</color>, \n" +
                                $"Patrol Range: {patrolRange:F1} -> <color=yellow>{nextLevel.patrolRange:F1}</color>, " +
                                $"Repair Rate: {repairRate:F1} -> <color=yellow>{nextLevel.repairRate:F1}</color>, " +
                                $"Move Speed: {moveSpeed:F1} -> <color=yellow>{nextLevel.moveSpeed:F1}</color>, \n" +
                                $"Placement Cost: {placementCost:F1} -> <color=yellow>{nextLevel.placementCost:F1}</color>," +
                                $"Energy Cost: {energyCost:F1} -> <color=yellow>{nextLevel.energyCost:F1}</color>";

        return nextLevel;
    }
}
