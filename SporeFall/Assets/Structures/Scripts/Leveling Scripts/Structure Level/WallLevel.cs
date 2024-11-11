using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WallLevel : StructureLevel
{
    [Header("Flamethrower Specific")]
    float minTimeToRegen;
    float wallRegen;
}
