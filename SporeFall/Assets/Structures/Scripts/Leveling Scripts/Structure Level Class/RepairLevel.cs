using System;
using System.Collections;
using System.Collections.Generic;
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
                                $"Health: {nextLevel.maxHealth:F1}, " +
                                $"Range: {nextLevel.healRange:F1}, " +
                                $"Heal Amount: {nextLevel.healAmount:F1}, " +
                                $"Heal Rate: {nextLevel.healRate:F1}, " +
                                $"Placement Cost: {nextLevel.placementCost:F1}, ||" +
                                $"Energy Cost: {nextLevel.energyCost:F1}";



        return nextLevel;
    }
}
