using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base stats that all structures share
[Serializable]
public abstract class StructureLevel
{
    [Header("Base Stats")]
    public int level;
    public float maxHealth;
    public float cost;
    public float energyCost;
    public string upgradeDescription;

    [Header("Leveling")]
    public float upgradeCostMultiplier = 1.5f;
    public float upgradeHealthMultiplier = 1.5f;
    public float upgradeEnergyCostMultiplier = 1.5f;

    public abstract StructureLevel NextLevel();
    public float GetUpgradeCost() => cost * upgradeCostMultiplier;
}







