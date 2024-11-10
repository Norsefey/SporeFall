using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base ScriptableObject for structure configurations
public abstract class StructureLevels : ScriptableObject
{
    public string structureName;
    public string description;
    public Sprite icon;

    // Override in derived classes to provide type-specific validation
    public abstract int GetLevelCount();
    public abstract StructureLevel GetLevel(int level);
}
// Specific ScriptableObjects for each structure type
[CreateAssetMenu(fileName = "TurretLevels", menuName = "Structures/Turret Levels")]
public class TurretLevels : StructureLevels
{
    public TurretLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}

[CreateAssetMenu(fileName = "RepairLevels", menuName = "Structures/Repair Levels")]
public class RepairLevels : StructureLevels
{
    public RepairLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}

[CreateAssetMenu(fileName = "FlameThrowerLevels", menuName = "Structures/FlameThrower Levels")]
public class FlameThrowerLevels : StructureLevels
{
    public FlameThrowerLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}

[CreateAssetMenu(fileName = "WallLevels", menuName = "Structures/Wall Levels")]
public class WallLevels : StructureLevels
{
    public WallLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
[CreateAssetMenu(fileName = "ShermanLevels", menuName = "Structures/Sherman Levels")]
public class ShermanLevels : StructureLevels
{
    public ShermanLevel[] levels;

    public override int GetLevelCount() => levels.Length;
    public override StructureLevel GetLevel(int level) => levels[level];
}
