using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MortyLevels", menuName = "Structures/Morty Levels")]
public class MortyLevels : StructureLevels
{
    public MortyLevel[] levels;
    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
