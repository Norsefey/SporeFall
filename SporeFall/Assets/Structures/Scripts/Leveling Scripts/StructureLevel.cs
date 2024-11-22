using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base stats that all structures share
[Serializable]
public class StructureLevel
{
    public string name;
    [Header("Base Stats")]
    public float maxHealth;
    public float cost;
    public float energyCost;

    public string upgradeDescription;
}







