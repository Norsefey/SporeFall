using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WallLevel : StructureLevel
{
    [Header("Base Wall Stats")]
    public float protectionRange = 5;
    [Range(0, 1)]
    public float damageReduction = .15f;

    [Header("Wall Leveling")]
    public float upgradeProtectionRangeIncrease = 1f;
    public float upgradeDamageReductionIncrease = .05f; // 5% increase per level, capped at 50%
    public float dmgReductionCap = .5f; // Cap damage reduction at 50%


    public override StructureLevel NextLevel()
    {
        WallLevel nextLevel = new WallLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            placementCost = this.placementCost * upgradePlacementCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
            upgradeCost = this.upgradeCost * upgradeUpgradeCostMultiplier,

            protectionRange = this.protectionRange + upgradeProtectionRangeIncrease,
            damageReduction = Mathf.Min(this.damageReduction + upgradeDamageReductionIncrease, dmgReductionCap) // Cap at 50% reduction  
        };

        nextLevel.upgradeDescription =
                                 $"Health: {nextLevel.maxHealth:F1}, " +
                                 $"Protection Range: {nextLevel.protectionRange:F1}, " +
                                 $"Damage Reduction: {nextLevel.damageReduction:P0} (capped at {nextLevel.dmgReductionCap:P0})" +
                                 $"Placement Cost: {nextLevel.placementCost:F1}, ||" +
                                 $"Energy Cost: {nextLevel.energyCost:F1}, ";
        return nextLevel;
    }
}
