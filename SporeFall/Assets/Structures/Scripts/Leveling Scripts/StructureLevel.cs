using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base stats that all structures share
[Serializable]
public abstract class StructureLevel
{
    [Header("Base Stats")]
    public int level = 1;
    public float maxHealth;
    public float placementCost;
    public float energyCost;
    public float upgradeCost = 50f;
    public string upgradeDescription;


    [Header("Leveling")]
    public float upgradeHealthMultiplier = 1;
    public float upgradePlacementCostMultiplier = 1;
    public float upgradeEnergyCostMultiplier = 1;
    public float upgradeUpgradeCostMultiplier = 1;

    public abstract StructureLevel NextLevel();
}







