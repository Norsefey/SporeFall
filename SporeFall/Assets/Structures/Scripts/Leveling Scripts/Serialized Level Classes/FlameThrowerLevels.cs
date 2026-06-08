using UnityEngine;

[CreateAssetMenu(fileName = "FlameThrowerLevels", menuName = "Structures/FlameThrower Levels")]
public class FlameThrowerLevels : StructureStats
{
    public FlameThrowerLevel baseStats;

    public override StructureLevel GetBaseLevel()
    {
        return baseStats;
    }
}
