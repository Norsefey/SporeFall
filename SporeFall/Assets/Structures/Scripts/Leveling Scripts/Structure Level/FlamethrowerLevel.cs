using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlameThrowerLevel : StructureLevel
{
    [Header("Flamethrower Specific")]
    public float damageAmount;
    public float range;
    public float damageTickRate;
}
