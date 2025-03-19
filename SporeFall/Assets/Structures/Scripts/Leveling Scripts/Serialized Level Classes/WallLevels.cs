using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WallLevels", menuName = "Structures/Wall Levels")]
public class WallLevels : StructureLevels
{
    public WallLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
