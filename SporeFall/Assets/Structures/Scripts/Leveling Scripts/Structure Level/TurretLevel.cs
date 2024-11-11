using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
