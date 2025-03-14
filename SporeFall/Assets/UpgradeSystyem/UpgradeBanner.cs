using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBanner : MonoBehaviour
{
    public TMP_Text typeText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public Button upgradeButton;

    private StructureType currentType;
    private StructureLevel currentLevel;
    private UpgradeManager UpgradeManager;
    public UpgradeUI upgradeUI;
    public void SetupBanner(StructureType type, StructureLevel level, UpgradeManager manager)
    {
        currentType = type;
        currentLevel = level;
        UpgradeManager = manager;
        UpdateBannerVisuals(type, level);
    }
    private void UpdateBannerVisuals(StructureType type, StructureLevel level)
    {

        typeText.text = $"{type.ToString()} : \n Current Lv {UpgradeManager.GetStructureLevel(type) + 1}";

        bool isMaxLevel = UpgradeManager.IsMaxLevel(type);

        if (isMaxLevel)
        {
            costText.text = "Max Level";
            //select The scroll bar
            upgradeButton.FindSelectableOnRight().Select();
            upgradeButton.interactable = false;
            descriptionText.text = "This structure is fully upgraded.";
        }
        else if (level != null)
        {
            costText.text = $"Mycelia: {level.cost}";
            upgradeButton.interactable = true;
            descriptionText.text = level.upgradeDescription;
        }

        // Ensure button doesn't keep old listeners
        upgradeButton.onClick.RemoveAllListeners();
        if (!isMaxLevel)
        {
            upgradeButton.onClick.AddListener(PerformUpgrade);
        }
    }
    void PerformUpgrade()
    {
        UpgradeManager upgradeManager = GameManager.Instance.upgradeManager;
        float currentMycelia = GameManager.Instance.Mycelia;

        if (upgradeManager.CanUpgrade(currentType, currentMycelia))
        {
            upgradeManager.UpgradeStructure(currentType, currentMycelia);
            GameManager.Instance.trainHandler.ApplyUpgradeToStructures();


            GameManager.Instance.DecreaseMycelia(currentLevel.cost);
            upgradeUI.UpdateMyceliaAmount();

            currentLevel = upgradeManager.GetNextLevel(currentType);

            UpdateBannerVisuals(currentType, currentLevel);
        }
    }
}
