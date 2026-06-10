using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBanner : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    public TMP_Text typeText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public Button upgradeButton;

    public StructureLevel structureLevel;
    private UpgradeManager upgradeManager;
    public UpgradeUI upgradeUI;
    private StructureType myType;


    public void SetupBanner(StructureType type, UpgradeManager manager)
    {
        myType = type;
        
        structureLevel = manager.GetStructureLevelOfType(type);
        upgradeManager = manager;
        UpdateBannerVisuals(structureLevel);
        upgradeButton.onClick.AddListener(PerformUpgrade);
    }
    private void UpdateBannerVisuals(StructureLevel level)
    {
        typeText.text = $"{myType.ToString()} : \n Lv {level.level} -> <color=yellow>{level.NextLevel().level}</color>";

        if (upgradeManager.CanUpgrade(myType, GameManager.Instance.Mycelia))
            costText.text = $"Mycelia: {level.upgradeCost:F1}";
        else
            costText.text = $"<color=red>Mycelia: {level.upgradeCost:F1}</color>";

        upgradeButton.interactable = upgradeManager.CanUpgrade(myType, GameManager.Instance.Mycelia);
        descriptionText.text = level.NextLevel().upgradeDescription;
    }
    void PerformUpgrade()
    {
        if(upgradeManager.CanUpgrade(myType, GameManager.Instance.Mycelia))
        {
            GameManager.Instance.DecreaseMycelia(structureLevel.upgradeCost);
            //upgradeUI.UpdateMyceliaAmount();
            upgradeUI.ShowStructureUpgrades(); // Refresh the upgrade banners to update costs and interactability

            StructureLevel newLevel = structureLevel.NextLevel();
            newLevel.upgradePlacementCostMultiplier = structureLevel.upgradePlacementCostMultiplier;
            newLevel.upgradeHealthMultiplier = structureLevel.upgradeHealthMultiplier;
            newLevel.upgradeEnergyCostMultiplier = structureLevel.upgradeEnergyCostMultiplier;
            newLevel.upgradeUpgradeCostMultiplier = structureLevel.upgradeUpgradeCostMultiplier;

            upgradeManager.UpgradeStructure(myType, newLevel);
            GameManager.Instance.ApplyUpgradeToStructures(myType, newLevel);

            UpdateBannerVisuals(newLevel);

            structureLevel = newLevel;
        }
    }
}
