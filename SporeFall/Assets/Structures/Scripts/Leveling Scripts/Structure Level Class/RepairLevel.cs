using System;
using UnityEngine;

[Serializable]
public class RepairLevel : StructureLevel
{
    [Header("Base Heal Station Stats")]
    public float healRange;
    public float healAmount;
    public float healRate;

    [Header("Repair Leveling")]
    public float upgradeHealRangeMultiplier = 1.2f;
    public float upgradeHealAmountMultiplier = 1.25f;
    public float upgradeHealRateMultiplier = 1.1f;

    public override StructureLevel NextLevel()
    {
        RepairLevel nextLevel = new RepairLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            placementCost = this.placementCost * upgradePlacementCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
            upgradeCost = this.upgradeCost * upgradeUpgradeCostMultiplier,

            healRange = this.healRange * upgradeHealRangeMultiplier,
            healAmount = this.healAmount * upgradeHealAmountMultiplier,
            healRate = this.healRate * upgradeHealRateMultiplier
        };

        nextLevel.upgradeDescription =
                                $"Health: {maxHealth:F1} -> <color=yellow>{nextLevel.maxHealth:F1}</color>, " +
                                $"Range: {healRange:F1} -> <color=yellow>{nextLevel.healRange:F1}</color>, " +
                                $"Heal Amount: {healAmount:F1} -> <color=yellow>{nextLevel.healAmount:F1}</color>, " +
                                $"Heal Rate: {healRate:F1} -> <color=yellow>{nextLevel.healRate:F1}</color>, " +
                                $"Placement Cost: {placementCost:F1} -> <color=yellow>{nextLevel.placementCost:F1}</color>," +
                                $"Energy Cost: {energyCost:F1} -> <color=yellow>{nextLevel.energyCost:F1}</color>";



        return nextLevel;
    }
}
