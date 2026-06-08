using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LilyLevels", menuName = "Structures/Lily Levels")]
public class LilyLevels : StructureStats
{
    public LilyLevel baseStats;

    public override StructureLevel GetBaseLevel()
    {
        return baseStats;
    }
}
