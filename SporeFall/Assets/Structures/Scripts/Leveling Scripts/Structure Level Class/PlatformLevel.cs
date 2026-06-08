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
                                $"Health: {nextLevel.maxHealth:F1}, \n" +
                                $"Placement Cost: {nextLevel.placementCost:F1}, ||" +
                                $"Energy Cost: {nextLevel.energyCost:F1}";

        return nextLevel;
    }
}
