using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MortyLevel : StructureLevel
{
    [Header("Base Morty Stats")]
    public float detectionRange = 20f;
    public float fireRate = 1f;         
    public float fireRange = 100f;

    [Header("Projectile Stats")]
    public ProjectileData projectileStats;

    [Header("Mortar Leveling")]
    public float upgradeDetectionRangeMultiplier = 1.2f;
    public float upgradeDamageMultiplier = 1.25f;


    public override StructureLevel NextLevel()
    {
        MortyLevel nextLevel = new MortyLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            cost = this.cost * upgradeCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,

            detectionRange = this.detectionRange * upgradeDetectionRangeMultiplier,
            projectileStats = new ProjectileData
            {
                Damage = this.projectileStats.Damage * upgradeDamageMultiplier,
                Speed = this.projectileStats.Speed,
                Lifetime = this.projectileStats.Lifetime
            },
        };

        nextLevel.upgradeDescription =
                                $"Health: {nextLevel.maxHealth:F1}, " +
                                $"Range: {nextLevel.fireRange:F1}, \n" +
                                $"Damage: {nextLevel.projectileStats.Damage:F1}, " +
                                $"Fire Rate: {nextLevel.fireRate:F1}, \n" +
                                $"Cost: {nextLevel.cost:F1}, " +
                                $"Energy Cost: {nextLevel.energyCost:F1}";

        return nextLevel;
    }
}
