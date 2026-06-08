
using UnityEngine;

public class FlameThrowerStatUpdater : MonoBehaviour, IStructureStats
{
    [SerializeField] private FlameThrower flamey;
    private FlameThrowerLevel currentLevel;
    public void Initialize(StructureLevel level)
    {
        if (level is FlameThrowerLevel flameThrowerLevel)
        {
            UpdateFlameyStats(flameThrowerLevel);
        }
    }

    public void UpdateStats(StructureLevel newLevel)
    {
        if (newLevel is FlameThrowerLevel flameThrowerLevel)
        {
            UpdateFlameyStats(flameThrowerLevel);
        }
    }

    private void UpdateFlameyStats(FlameThrowerLevel levelData)
    {
        currentLevel = levelData;

        flamey.damageAmount = levelData.damage;
        flamey.range = levelData.range;
        flamey.damageTickRate = levelData.damageTickRate;

    }
}
