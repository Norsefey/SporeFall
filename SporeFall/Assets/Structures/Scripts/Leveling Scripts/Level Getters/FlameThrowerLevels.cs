using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlameThrowerLevels", menuName = "Structures/FlameThrower Levels")]
public class FlameThrowerLevels : StructureLevels
{
    public FlameThrowerLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
