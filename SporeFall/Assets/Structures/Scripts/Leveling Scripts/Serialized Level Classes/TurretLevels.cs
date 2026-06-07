using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Specific ScriptableObjects for each structure type
[CreateAssetMenu(fileName = "TurretLevels", menuName = "Structures/Turret Levels")]
public class TurretLevels : StructureStats
{
    public TurretLevel baseStats;

    private void OnEnable()
    {
        currentLevel = baseStats;
    }
    //public TurretLevel[] levels;
}
