using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Specific ScriptableObjects for each structure type
[CreateAssetMenu(fileName = "TurretLevels", menuName = "Structures/Turret Levels")]
public class TurretLevels : StructureLevels
{
    public TurretLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
