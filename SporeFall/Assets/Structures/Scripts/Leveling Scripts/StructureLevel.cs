using System;
using UnityEngine;

// Base stats that all structures share
[System.Serializable]
public abstract class StructureLevel
{
    [Header("Base Stats")]
    public  int level = 1;
    public float maxHealth;
    public float placementCost;
    public float energyCost;
    public float upgradeCost = 50f;
    public string upgradeDescription;


    [Header("Leveling")]
    public float upgradeHealthMultiplier;
    public float upgradePlacementCostMultiplier;
    public float upgradeEnergyCostMultiplier;
    public float upgradeUpgradeCostMultiplier;

    public abstract StructureLevel NextLevel();
}







