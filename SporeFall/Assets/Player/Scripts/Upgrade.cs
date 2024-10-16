using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Upgrade
{
    public string upgradeName;
    public string description;
    public float currentValue;
    public float upgradeAmount;
    public float cost;
    public int level = 0;
}
