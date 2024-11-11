using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlameThrowerLevel : StructureLevel
{
    [Header("Flamethrower Specific")]
    public float damagePerSecond;
    public float range;
    public float tickRate;
}
