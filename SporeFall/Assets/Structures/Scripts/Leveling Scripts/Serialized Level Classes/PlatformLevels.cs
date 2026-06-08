
using UnityEngine;
[CreateAssetMenu(fileName = "PlatformLevels", menuName = "Structures/Platform Levels")]
public class PlatformLevels : StructureStats
{
    public PlatformLevel baseLevel;

    public override StructureLevel GetBaseLevel()
    {
        return baseLevel;
    }
}
