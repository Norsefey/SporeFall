using System;
using System.Collections;
using System.Collections.Generic;
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
/*    public float upgradeMoveSpeedMultiplier = 1.1f; // 10% increase in move speed per level
    public float upgradeTurnSpeedMultiplier = 1.1f; // 10% increase in turn speed per level
    public float upgradeChangeDirectionIntervalMultiplier = 1.1f; // 10% increase in change direction interval per level
    public float upgradeDamageMultiplier = 1.2f; // 20% increase in damage per level
    public float upgradeDetectionRadiusMultiplier = 1.1f; // 10% increase in detection radius per level
    public float upgradeEnemyInfluenceWeightMultiplier = 1.1f; // 10% increase in enemy influence weight per level
    public float upgradeRandomMovementWeightMultiplier = 1.1f; // 10% increase in random movement weight per level
    public float upgradeExplosionRadiusMultiplier = 1.1f; // 10% increase in explosion radius per level
*/
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
                                $"Health: {nextLevel.maxHealth:F1}, " +
                                $"Max Active Shermans: {nextLevel.maxActiveShermans}, \n" +
                                $"Placement Cost: {nextLevel.placementCost:F1}, ||" +
                                $"Energy Cost: {nextLevel.energyCost:F1}";

        return nextLevel;
    }
}
