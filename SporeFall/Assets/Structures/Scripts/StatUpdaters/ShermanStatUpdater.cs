using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShermanStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private ShermanStructureControls ShermanStation;
    private ShermanLevel currentLevel;
    public void Initialize(StructureLevel level)
    {
        if (level is ShermanLevel shermanLevel)
        {
            UpdateShermanStats(shermanLevel);
        }
    }

    public void UpdateStats(StructureLevel newLevel)
    {
        if(newLevel is ShermanLevel shermanLevel)
        {
            UpdateShermanStats(shermanLevel);
        }
    }

    private void UpdateShermanStats(ShermanLevel levelData)
    {
        currentLevel = levelData;

        ShermanStation.maxActiveShermans = levelData.maxActiveShermans;
        foreach(var sherman in ShermanStation.currentShermans)
        {
            sherman.UpdateVisual(levelData.level);
            sherman.moveSpeed = levelData.moveSpeed;
            sherman.turnSpeed = levelData.turnSpeed;
            sherman.changeDirectionInterval = levelData.changeDirectionInterval;
            sherman.damage = levelData.damage;
            sherman.detectionRadius = levelData.detectionRadius;
            sherman.randomMovementWeight = levelData.enemyInfluenceWeight;
            sherman.randomMovementWeight = levelData.randomMovementWeight;
            sherman.explosionRadius = levelData.explosionRadius;
            sherman.damageFalloff = levelData.damageFalloff;
        }
        if (gameObject.activeSelf)
            StartCoroutine(ShermanStation.ResetShermanBots());
    }
}
