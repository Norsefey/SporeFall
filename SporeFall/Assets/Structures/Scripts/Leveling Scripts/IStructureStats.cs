using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base structure behavior interface
public interface IStructureStats
{
    void Initialize(StructureLevels levels, int level, float waveMultiplier);
    void UpdateStats(StructureLevels levels, int level, float waveMultiplier);
}
