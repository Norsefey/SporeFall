using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base stats that all structures share
[Serializable]
public class StructureLevel
{
    public string name;
    [Header("Base Stats")]
    public float maxHealth;
    public float cost;
    public float energyCost;
}
// Specific stat extensions for different structure types
[Serializable]
public class TurretLevel : StructureLevel
{
    [Header("Turret Stats")]
    public float detectionRange = 20f;       // How far the turret can detect enemies
    public float rotationSpeed = 5f;         // Speed of the turret rotation
    public float fireRate = 1f;              // How often the turret fires
    public float fireRange = 100f;        // Maximum range of raycast
    public float fireCooldown = 0f;

    [Header("Projectile Stats")]
    public float speed;
    public float damage;
    public float lifetime;
    public bool useGravity;
    public float arcHeight;
    public bool canBounce;
    public int maxBounces;
    public float bounceDamageMultiplier;
}
[Serializable]
public class RepairLevel : StructureLevel
{
    [Header("Repair Specific")]
    public float healRange;
    public float healAmount;
    public float healRate;
}

[Serializable]
public class FlameThrowerLevel : StructureLevel
{
    [Header("Flamethrower Specific")]
    public float damagePerSecond;
    public float range;
    public float tickRate;
}

[Serializable]
public class WallLevel : StructureLevel
{
    [Header("Flamethrower Specific")]
    float minTimeToRegen;
    float wallRegen;
}

[Serializable]
public class ShermanLevel : StructureLevel
{
    [Header("Sherman Specific")]
    public float moveSpeed = 2f;
    public float turnSpeed = 1f;    // How fast the object changes direction
    public float changeDirectionInterval = 2f;    // How often to change direction
    public float damage = 100;    // Damage dealt to enemies on contact
    public float detectionRadius = 10f;    // Detection radius for nearby enemies
    public float enemyInfluenceWeight = 2f;    // Weight for enemy influence on direction (higher = more attracted to enemies)
    public float randomMovementWeight = 1f;    // Weight for random movement (higher = more random movement)
    public float explosionRadius = 10f;
    public AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 0f, 0f);
}
