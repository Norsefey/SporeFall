using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StanleyLevels", menuName = "Structures/Stanley Levels")]
public class StanleyLevels : StructureStats
{
    public StanleyLevel baseLevel;

    public override StructureLevel GetBaseLevel()
    {
        return baseLevel;
    }
}
