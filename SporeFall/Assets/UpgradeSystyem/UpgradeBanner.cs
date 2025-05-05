using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    private StructureType currentType;
    private StructureLevel currentLevel;
    private UpgradeManager upgradeManager;
    public UpgradeUI upgradeUI;
    public void SetupBanner(StructureType type, StructureLevel level, UpgradeManager manager)
    {
        currentType = type;
        currentLevel = level;
        upgradeManager = manager;
        UpdateBannerVisuals(type, level);
    }
    private void UpdateBannerVisuals(StructureType type, StructureLevel level)
    {

        typeText.text = $"{type.ToString()} : \n Current Lv {upgradeManager.GetStructureLevel(type) + 1}";

        bool isMaxLevel = upgradeManager.IsMaxLevel(type);

        if (isMaxLevel)
        {
            costText.text = "Max Level";
            //select The scroll bar
            //upgradeButton.FindSelectableOnRight().Select();
            upgradeButton.interactable = false;
            textMove.canMove = false;
            buttonText.color = DarkRed;
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
        float currentMycelia = GameManager.Instance.Mycelia;

        if (upgradeManager.CanUpgrade(currentType, currentMycelia))
        {
            upgradeManager.UpgradeStructure(currentType, currentMycelia);
            
            GameManager.Instance.ApplyUpgradeToStructures();
            GameManager.Instance.DecreaseMycelia(currentLevel.cost);
            
            upgradeUI.UpdateMyceliaAmount();

            currentLevel = upgradeManager.GetNextLevel(currentType);

            UpdateBannerVisuals(currentType, currentLevel);
        }
    }
}
