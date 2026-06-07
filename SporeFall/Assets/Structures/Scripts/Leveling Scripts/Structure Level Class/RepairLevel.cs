using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RepairLevel : StructureLevel
{
    [Header("Base Heal Station Stats")]
    public float healRange;
    public float healAmount;
    public float healRate;

    public override StructureLevel NextLevel()
    {
        throw new NotImplementedException();
    }
}
