using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base ScriptableObject for structure configurations
public abstract class StructureLevels : ScriptableObject
{
    public StructureType type;
    public string structureName;
    public string description;
    public Sprite icon;

    // Override in derived classes to provide type-specific validation
    public abstract int GetLevelCount();
    public abstract StructureLevel GetLevel(int level);
}
