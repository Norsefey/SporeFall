using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LilyLevels", menuName = "Structures/Lily Levels")]
public class LilyLevels : StructureLevels
{
    public LilyLevel[] levels;
    public override StructureLevel GetLevel(int level) => levels[level];
    public override int GetLevelCount() => levels.Length;
}
