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
}
