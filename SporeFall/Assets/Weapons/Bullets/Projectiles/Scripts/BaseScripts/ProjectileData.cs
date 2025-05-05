using UnityEngine;

// Projectile data structure
[System.Serializable]
public struct ProjectileData
{
    [Header("Base Properties")]
    public float Speed;
    public float Lifetime;
    public float Damage;

    [Header("Movement Properties")]
    public bool UseGravity;
    public bool UseArcTrajectory;
    public float ArcHeight;

    [Header("Bounce Properties")]
    public bool CanBounce;
    public int MaxBounces;
    public float BounceDamageMultiplier;

    [Header("Target Information")]
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public Vector3 TargetPosition;
}

public enum ProjectileType
{
    Standard,
    Explosive,
    DOT,
    Corrupted,
    Spawner
}
