using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WallLevels", menuName = "Structures/Wall Levels")]
public class WallLevels : StructureStats
{
    public WallLevel baseLevel;

    public override StructureLevel GetBaseLevel()
    {
        return baseLevel;
    }
}
