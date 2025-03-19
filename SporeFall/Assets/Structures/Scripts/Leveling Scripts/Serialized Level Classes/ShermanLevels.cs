using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShermanLevels", menuName = "Structures/Sherman Levels")]
public class ShermanLevels : StructureLevels
{
    public ShermanLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
