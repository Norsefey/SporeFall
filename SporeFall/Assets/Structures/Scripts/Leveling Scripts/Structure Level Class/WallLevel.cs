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

    public override StructureLevel NextLevel()
    {
        StructureLevel nextLevel = new WallLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            cost = this.cost * upgradeCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
            
            protectionRange = this.protectionRange + upgradeProtectionRangeIncrease,
            damageReduction = Mathf.Min(this.damageReduction + upgradeDamageReductionIncrease, .5f), // Cap at 50% reduction
            
            upgradeDescription = $"Upgrade to Level: {level} " +
                                 $"Health: {maxHealth:F1}, " +
                                 $"Cost: {cost:F1}, " +
                                 $"Energy Cost: {energyCost:F1}, " +
                                 $"Protection Range: {protectionRange:F1}, " +
                                 $"Damage Reduction: {damageReduction:P0} (capped at 50%)"
        };
        return nextLevel;
    }
}
