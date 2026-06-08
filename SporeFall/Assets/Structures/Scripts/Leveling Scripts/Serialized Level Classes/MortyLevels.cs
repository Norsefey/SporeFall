using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MortyLevels", menuName = "Structures/Morty Levels")]
public class MortyLevels : StructureStats
{
    public MortyLevel baseStats;

    public override StructureLevel GetBaseLevel()
    {
        return baseStats;
    }
}
