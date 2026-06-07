using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShermanLevels", menuName = "Structures/Sherman Levels")]
public class ShermanLevels : StructureStats
{
    public ShermanLevel baseLevel;

    private void OnEnable()
    {
        currentLevel = baseLevel;
    }

    //public ShermanLevel[] levels;
}
