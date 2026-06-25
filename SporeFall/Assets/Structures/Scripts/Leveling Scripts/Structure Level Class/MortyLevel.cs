using System;
using UnityEngine;

[Serializable]
public class MortyLevel : StructureLevel
{
    [Header("Base Morty Stats")]
    public float detectionRange = 20f;
    public float fireRate = .02f;         
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
            placementCost = this.placementCost * upgradePlacementCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
            upgradeCost = this.upgradeCost * upgradeUpgradeCostMultiplier,
            
            fireRate = this.fireRate,
            fireRange = this.fireRange,

            detectionRange = this.detectionRange * upgradeDetectionRangeMultiplier,
            projectileStats = new ProjectileData
            {
                Speed = this.projectileStats.Speed,
                Lifetime = this.projectileStats.Lifetime,
                Corruption = this.projectileStats.Corruption,
                UseGravity = this.projectileStats.UseGravity,
                UseArcTrajectory = this.projectileStats.UseArcTrajectory,
                ArcHeight = this.projectileStats.ArcHeight,
                CanBounce = this.projectileStats.CanBounce,
                MaxBounces = this.projectileStats.MaxBounces,
                BounceDamageMultiplier = this.projectileStats.BounceDamageMultiplier,

                Damage = this.projectileStats.Damage * upgradeDamageMultiplier,
            },
        };

        nextLevel.upgradeDescription =
                                $"Health: {maxHealth:F1} -> <color=yellow>{nextLevel.maxHealth:F1}</color>, " +
                                $"Range: {fireRange:F1} -> <color=yellow>{nextLevel.fireRange:F1}</color>, \n" +
                                $"Damage: {projectileStats.Damage:F1} -> <color=yellow>{nextLevel.projectileStats.Damage:F1}</color>, " +
                                $"Fire Rate: {fireRate:F1} -> <color=yellow>{nextLevel.fireRate:F1}</color>, \n" +
                                $"Placement Cost: {placementCost:F1} -> <color=yellow>{nextLevel.placementCost:F1}</color>," +
                                $"Energy Cost: {energyCost:F1} -> <color=yellow>{nextLevel.energyCost:F1}</color>";

        return nextLevel;
    }
}
