using System;
using UnityEngine;

[Serializable]
public class PlatformLevel : StructureLevel
{


    public override StructureLevel NextLevel()
    {
        StructureLevel nextLevel = new PlatformLevel
        {
            level = this.level + 1,
            maxHealth = this.maxHealth * upgradeHealthMultiplier,
            placementCost = this.placementCost * upgradePlacementCostMultiplier,
            energyCost = this.energyCost * upgradeEnergyCostMultiplier,
            upgradeCost = this.upgradeCost * upgradeUpgradeCostMultiplier,


        };

        nextLevel.upgradeDescription =
                                $"Health: {maxHealth:F1} -> <color=yellow>{nextLevel.maxHealth:F1}</color>, \n" +
                                $"Placement Cost: {placementCost:F1} -> <color=yellow>{nextLevel.placementCost:F1}</color>," +
                                $"Energy Cost: {energyCost:F1} -> <color=yellow>{nextLevel.energyCost:F1}</color>";

        return nextLevel;
    }
}
