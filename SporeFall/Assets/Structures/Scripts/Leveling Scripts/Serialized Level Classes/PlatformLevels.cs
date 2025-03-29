using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PlatformLevels", menuName = "Structures/Platform Levels")]
public class PlatformLevels : StructureLevels
{
    public PlatformLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
