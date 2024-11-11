using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RepairLevels", menuName = "Structures/Repair Levels")]
public class RepairLevels : StructureLevels
{
    public RepairLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
