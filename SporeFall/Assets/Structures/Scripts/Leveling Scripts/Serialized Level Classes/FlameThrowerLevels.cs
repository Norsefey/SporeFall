using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlameThrowerLevels", menuName = "Structures/FlameThrower Levels")]
public class FlameThrowerLevels : StructureStats
{
    public FlameThrowerLevel baseStats;

    private void OnEnable()
    {
        currentLevel = baseStats;
    }

    //public FlameThrowerLevel[] levels;
}
