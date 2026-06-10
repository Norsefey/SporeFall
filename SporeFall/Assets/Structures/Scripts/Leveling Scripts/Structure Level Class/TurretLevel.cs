using UnityEngine;

// Specific stat extensions for different structure types
[System.Serializable]
public class TurretLevel : StructureLevel
{
    [Header("Base Turret Stats")]
    public float range = 20f;       // How far the turret can detect enemies
    public float rotationSpeed = 5f;         // Speed of the turret rotation
    public float fireRate = 1f;              // How often the turret fires
    public float fireRange = 100f;        // Maximum range of raycast
    public int ammoCapacity = 10;            // Number of shots before reload
    public float reloadTime = 2f;           // Time it takes to reload

    [Header("Base Projectile Stats")]
    public float speed;
    public float damage;
    public float lifetime;
    public bool useGravity;
    public float arcHeight;
    public bool canBounce;
    public int maxBounces;
    public float bounceDamageMultiplier;

    [Header("Turret Leveling")]
    public float upgradeDamageMultiplier = 1.5f;
    public float upgradeFireRateMultiplier = 1.2f;
    public float upgradeDetectionRangeIncrease = 2f;

    public override StructureLevel NextLevel()
    {
        TurretLevel nextLevel = new TurretLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            upgradeCost = this.upgradeCost * upgradeUpgradeCostMultiplier,

            placementCost = this.placementCost * upgradePlacementCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,

            rotationSpeed = this.rotationSpeed,
            fireRange = this.fireRange,
            ammoCapacity = this.ammoCapacity,
            reloadTime = this.reloadTime,
            speed = this.speed,
            lifetime = this.lifetime,
            useGravity = this.useGravity,
            arcHeight = this.arcHeight,
            canBounce = this.canBounce,
            maxBounces = this.maxBounces,
            bounceDamageMultiplier = this.bounceDamageMultiplier,

            damage = this.damage * upgradeDamageMultiplier,
            fireRate = this.fireRate * upgradeFireRateMultiplier,
            range = this.range + upgradeDetectionRangeIncrease
        };

        nextLevel.upgradeDescription =
                                 $"Health:{maxHealth:F1} -> <color=yellow>{nextLevel.maxHealth:F1}</color>, " +
                                 $"Range: {range:F1} -> <color=yellow>{nextLevel.range:F1}</color>, \n" +
                                 $"Damage: {damage:F1} -> <color=yellow>{nextLevel.damage:F1}</color>, " +
                                 $"Fire Rate: {fireRate:F1} -> <color=yellow>{nextLevel.fireRate:F1}</color>, \n"+
                                 $"Placement Cost: {placementCost:F1} -> <color=yellow>{nextLevel.placementCost:F1}</color>" ;
        return nextLevel;
    }
}
