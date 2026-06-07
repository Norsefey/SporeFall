using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StanleyLevels", menuName = "Structures/Stanley Levels")]
public class StanleyLevels : StructureStats
{
    public StanleyLevel baseLevel;

    private void OnEnable()
    {
        currentLevel = baseLevel;
    }

    //public StanleyLevel[] levels;
}
