using System;
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
                                $"Health: {maxHealth:F1} -> <color=yellow>{nextLevel.maxHealth:F1}</color>, " +
                                $"Range: {range:F1} -> <color=yellow>{nextLevel.range:F1}</color>, \n" +
                                $"Damage: {damage:F1} -> <color=yellow>{nextLevel.damage:F1}</color>, \n" +
                                $"Placement Cost: {placementCost:F1} -> <color=yellow>{nextLevel.placementCost:F1}</color>," +
                                $"Energy Cost: {energyCost:F1} -> <color=yellow>{nextLevel.energyCost:F1}</color>";
        return nextLevel;
    }
}
