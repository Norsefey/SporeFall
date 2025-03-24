using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShermanStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private ShermanStructureControls ShermanStation;
    public void Initialize(StructureLevels levels, int level)
    {
        if (levels is ShermanLevels shermanLevels)
        {
            UpdateShermanStats(shermanLevels, level);
        }
    }

    public void UpdateStats(StructureLevels levels, int level)
    {
        if(levels is ShermanLevels shermanLevels)
        {
            UpdateShermanStats(shermanLevels, level);
        }
    }

    private void UpdateShermanStats(ShermanLevels levels, int level)
    {
        var levelData = levels.levels[level];

        ShermanStation.maxActiveShermans = levelData.maxActiveShermans;
        foreach(var sherman in ShermanStation.currentShermans)
        {
            sherman.UpdateVisual(level);
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
