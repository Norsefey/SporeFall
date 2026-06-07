using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeBanner : MonoBehaviour
{

    [SerializeField] private ButtonTextMovement textMove;
    Color DarkRed = new Color(0.3886792f, 0.1283374f, 0.1283374f);

    [SerializeField] private TMP_Text buttonText;
    public TMP_Text typeText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public Button upgradeButton;

    private StructureStats structureStats;
    private StructureLevel nextLevel;
    private UpgradeManager upgradeManager;
    public UpgradeUI upgradeUI;
    public void SetupBanner(StructureStats stats, UpgradeManager manager)
    {
        structureStats = stats;
        nextLevel = stats.currentLevel.NextLevel();
        upgradeManager = manager;
        UpdateBannerVisuals(stats);
        upgradeButton.onClick.AddListener(PerformUpgrade);
    }
    private void UpdateBannerVisuals(StructureStats stats)
    {
        typeText.text = $"{stats.type.ToString()} : \n Lv {stats.currentLevel.level} TO-> {nextLevel.level}";

        if (nextLevel != null)
        {
            costText.text = $"Mycelia: {nextLevel.upgradeCost:F1}";
            upgradeButton.interactable = true;
            descriptionText.text = nextLevel.upgradeDescription;
        }

        // Ensure button doesn't keep old listeners
        //upgradeButton.onClick.RemoveAllListeners();
    }
    void PerformUpgrade()
    {        
        float currentMycelia = GameManager.Instance.Mycelia;

        if (upgradeManager.CanUpgrade(structureStats.type, currentMycelia))
        {
            GameManager.Instance.DecreaseMycelia(nextLevel.upgradeCost);
            upgradeUI.UpdateMyceliaAmount();

            structureStats.currentLevel = nextLevel;
            nextLevel = nextLevel.NextLevel();

            GameManager.Instance.ApplyUpgradeToStructures();
           
            UpdateBannerVisuals(structureStats);
        }
    }
}
