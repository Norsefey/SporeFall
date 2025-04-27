using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MortyLevel : StructureLevel
{
    [Header("Morty Stats")]
    public float detectionRange = 20f;
    public float fireRate = 1f;         
    public float fireRange = 100f;

    [Header("Projectile Stats")]
    public ProjectileData projectileStats;
}
