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
    public float upgradeRangeMultiplier = 1.1f;


    public override StructureLevel NextLevel()
    {
        FlameThrowerLevel nextLevel = new FlameThrowerLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            cost = this.cost * upgradeCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
           
            damage = this.damage * upgradeDamageMultiplier,
            range = this.range * upgradeRangeMultiplier,
            damageTickRate = this.damageTickRate, // tick rate does not change with level
        };

        nextLevel.upgradeDescription =
                                $"Health: {nextLevel.maxHealth:F1}, " +
                                $"Range: {nextLevel.range:F1}, \n" +
                                $"Damage: {nextLevel.damage:F1}, \n" +
                                $"Cost: {nextLevel.cost:F1}, " +
                                $"Energy Cost: {nextLevel.energyCost:F1}";
        return nextLevel;
    }
}
