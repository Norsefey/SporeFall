using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBanner : MonoBehaviour
{
    public StructureLevels levels;

    public TMP_Text typeText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public Button upgradeButton;

    private StructureType currentType;
    private StructureLevel currentLevel;
    public UpgradeUI upgradeUI;
    public void SetupBanner(StructureType type, StructureLevel level)
    {
        currentType = type;
        currentLevel = level;

        UpdateBannerVisuals(type, level);
    }
    private void UpdateBannerVisuals(StructureType type, StructureLevel level)
    {
        typeText.text = $"{type.ToString()} : \n Current Lv {GameManager.Instance.UpgradeManager.GetStructureLevel(type) + 1}";

        // Check if this is the max level
        bool isMaxLevel = GameManager.Instance.UpgradeManager.IsMaxLevel(type);

        if (isMaxLevel)
        {
            costText.text = "Max Level";
            upgradeButton.interactable = false;
        }
        else
        {
            costText.text = $"Mycelia: {level.cost}";
            upgradeButton.interactable = true;
            descriptionText.text = level.upgradeDescription;
        }
        // Remove any existing listeners to prevent multiple calls
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(PerformUpgrade);
    }

    void PerformUpgrade()
    {
        if (GameManager.Instance.UpgradeManager.CanUpgrade(currentType, GameManager.Instance.TrainHandler.players[0].Mycelia))
        {
            GameManager.Instance.UpgradeManager.UpgradeStructure(currentType, GameManager.Instance.TrainHandler.players[0].Mycelia);
            GameManager.Instance.TrainHandler.ApplyUpgradeToStructures();
            upgradeUI.UpdateMyceliaAmount();
            // Get the next level after upgrade
            StructureLevel nextLevel = GameManager.Instance.UpgradeManager.GetNextLevel(currentType);
            // Update the banner visuals
            UpdateBannerVisuals(currentType, nextLevel);
        }
    }
}
