using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LilyLevel : StructureLevel
{
    [Header("Base Lily Stats")]
    public int maxActiveLilies = 1;
    public float patrolRange = 7.5f;
    public float repairRate = 7;
    public float moveSpeed = 2f;

    public override StructureLevel NextLevel()
    {
        throw new NotImplementedException();
    }
}
