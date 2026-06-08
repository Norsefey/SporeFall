using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base ScriptableObject for structure configurations
public abstract class StructureStats : ScriptableObject
{
    public StructureType type;
    public string structureName;
    public string description;
    public Sprite icon;

    public abstract StructureLevel GetBaseLevel();
}
