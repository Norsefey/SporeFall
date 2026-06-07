using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlameThrowerLevel : StructureLevel
{
    [Header("Base Flamethrower Stats")]
    public float damage;
    public float range;
    public float damageTickRate;

    [Header("Flamethrower Leveling")]
    public float upgradeDamageMultiplier = 1.5f;

    public override StructureLevel NextLevel()
    {
        FlameThrowerLevel nextLevel = new FlameThrowerLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            placementCost = this.placementCost * upgradePlacementCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
            upgradeCost = this.upgradeCost * upgradeUpgradeCostMultiplier,

            damage = this.damage * upgradeDamageMultiplier,
            range = this.range,
            damageTickRate = this.damageTickRate, // tick rate does not change with level
        };

        nextLevel.upgradeDescription =
                                $"Health: {nextLevel.maxHealth:F1}, " +
                                $"Range: {nextLevel.range:F1}, \n" +
                                $"Damage: {nextLevel.damage:F1}, \n" +
                                $"Placement Cost: {nextLevel.placementCost:F1}, ||" +
                                $"Energy Cost: {nextLevel.energyCost:F1}";
        return nextLevel;
    }
}
