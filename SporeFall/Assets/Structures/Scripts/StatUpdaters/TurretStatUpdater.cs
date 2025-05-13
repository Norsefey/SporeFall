using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private Turret turret;
    public void Initialize(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is TurretLevels turretLevels)
        {
            UpdateTurretStats(turretLevels, level, waveMultiplier);
        }
    }
    public void UpdateStats(StructureLevels levels, int level, float waveMultiplier)
    {
        if (levels is TurretLevels turretLevels)
        {
            UpdateTurretStats(turretLevels, level, waveMultiplier);
        }
    }

    private void UpdateTurretStats(TurretLevels levels, int level, float waveMultiplier)
    {
        var levelData = levels.levels[level];
        turret.rotationSpeed = levelData.rotationSpeed;
        turret.detectionRange = levelData.detectionRange;
        turret.fireRate = levelData.fireRate;
        turret.fireRange = levelData.fireRange;
        // Set up bullet data
        turret.bulletData = new ProjectileData 
        {
            Speed = levelData.speed,
            Damage = levelData.damage * waveMultiplier,
            Lifetime = levelData.lifetime,
            UseGravity = levelData.useGravity,
            ArcHeight = levelData.arcHeight,
            CanBounce = levelData.canBounce,
            MaxBounces = levelData.maxBounces,
            BounceDamageMultiplier = levelData.bounceDamageMultiplier
        };
    }

 
}
