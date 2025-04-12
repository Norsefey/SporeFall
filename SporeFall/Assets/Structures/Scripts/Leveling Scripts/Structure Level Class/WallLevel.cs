using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WallLevel : StructureLevel
{
    public float protectionRange = 5;
    [Range(0, 1)]
    public float damageReduction = .15f;
}
