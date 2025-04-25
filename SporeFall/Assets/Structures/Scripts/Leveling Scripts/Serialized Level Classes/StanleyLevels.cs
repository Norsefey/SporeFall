using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StanleyLevels", menuName = "Structures/Stanley Levels")]
public class StanleyLevels : StructureLevels
{
    public StanleyLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
