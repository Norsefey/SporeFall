using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RepairLevels", menuName = "Structures/Repair Levels")]
public class RepairLevels : StructureStats
{
    public RepairLevel baseLevel;

    private void OnEnable()
    {
        currentLevel = baseLevel;
    }

    //public RepairLevel[] levels;
}
