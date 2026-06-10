using System;
using UnityEngine;


[Serializable]
public class ShermanLevel : StructureLevel
{
    [Header("Sherman Specific")]
    public int maxActiveShermans = 1;
    public float moveSpeed = 2f;
    public float turnSpeed = 1f;    // How fast the object changes direction
    public float changeDirectionInterval = 2f;    // How often to change direction
    public float damage = 100;    // Damage dealt to enemies on contact
    public float detectionRadius = 10f;    // Detection radius for nearby enemies
    public float enemyInfluenceWeight = 2f;    // Weight for enemy influence on direction (higher = more attracted to enemies)
    public float randomMovementWeight = 1f;    // Weight for random movement (higher = more random movement)
    public float explosionRadius = 10f;
    public AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 0f, 0f);

    [Header("Sherman Leveling")]
    public int upgradeMaxActiveShermans = 99; // Max limit for active Shermans
    public override StructureLevel NextLevel()
    {
        ShermanLevel nextLevel = new ShermanLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            placementCost = this.placementCost * upgradePlacementCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
            upgradeCost = this.upgradeCost * upgradeUpgradeCostMultiplier,

            moveSpeed = this.moveSpeed,
            turnSpeed = this.turnSpeed,
            changeDirectionInterval = this.changeDirectionInterval,
            damage = this.damage,
            detectionRadius = this.detectionRadius,
            enemyInfluenceWeight = this.enemyInfluenceWeight,
            randomMovementWeight = this.randomMovementWeight,
            explosionRadius = this.explosionRadius,
            damageFalloff = this.damageFalloff,

            maxActiveShermans = Math.Min(this.maxActiveShermans + 1, upgradeMaxActiveShermans)
        };

        nextLevel.upgradeDescription =
                                $"Health: {maxHealth:F1} -> <color=yellow>{nextLevel.maxHealth:F1}</color>, " +
                                $"Max Active Shermans: {maxActiveShermans} -> <color=yellow>{nextLevel.maxActiveShermans}</color>, \n" +
                                $"Placement Cost: {placementCost:F1} -> <color=yellow>{nextLevel.placementCost:F1}</color>," +
                                $"Energy Cost: {energyCost:F1} -> <color=yellow>{nextLevel.energyCost:F1}</color>";

        return nextLevel;
    }
}
