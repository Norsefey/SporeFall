using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private Turret turret;
    private TurretLevel currentLevel;

    private void Awake()
    {
        currentLevel = GetComponent<Structure>().structureStats.GetBaseLevel() as TurretLevel;
    }

    public void Initialize(StructureLevel level)
    {
        if (level is TurretLevel turretLevel)
        {
            UpdateTurretStats(turretLevel);
        }
    }
    public void UpdateStats(StructureLevel newLevel)
    {
        if (newLevel is TurretLevel turretLevel)
        {
            UpdateTurretStats(turretLevel);
        }
    }

    private void UpdateTurretStats(TurretLevel levelData)
    {
        currentLevel = levelData;

        turret.rotationSpeed = levelData.rotationSpeed;
        turret.detectionRange = levelData.range;
        turret.fireRate = levelData.fireRate;
        turret.fireRange = levelData.fireRange;
        turret.ammoCapacity = levelData.ammoCapacity;
        turret.reloadTime = levelData.reloadTime;
        // Set up bullet data
        turret.bulletData = new ProjectileData 
        {
            Speed = levelData.speed,
            Damage = levelData.damage,
            Lifetime = levelData.lifetime,
            UseGravity = levelData.useGravity,
            ArcHeight = levelData.arcHeight,
            CanBounce = levelData.canBounce,
            MaxBounces = levelData.maxBounces,
            BounceDamageMultiplier = levelData.bounceDamageMultiplier
        };
    }

 
}
