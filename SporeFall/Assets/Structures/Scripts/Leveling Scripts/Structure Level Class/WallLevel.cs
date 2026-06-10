using System;
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
                                 $"Health:{maxHealth:F1} -> <color=yellow>{nextLevel.maxHealth:F1}</color>, " +
                                 $"Protection Range: {protectionRange:F1} -> <color=yellow>{nextLevel.protectionRange:F1}</color>, " +
                                 $"Damage Reduction: {damageReduction:P0} -> <color=yellow>{nextLevel.damageReduction:P0}</color> (capped at {nextLevel.dmgReductionCap:P0})" +
                                 $"Placement Cost: {placementCost:F1} -> <color=yellow>{nextLevel.placementCost:F1}</color>," +
                                 $"Energy Cost: {energyCost:F1} -> <color=yellow>{nextLevel.energyCost:F1}</color>, ";
        return nextLevel;
    }
}
