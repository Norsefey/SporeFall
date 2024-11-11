using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RepairLevel : StructureLevel
{
    [Header("Repair Specific")]
    public float healRange;
    public float healAmount;
    public float healRate;
}
