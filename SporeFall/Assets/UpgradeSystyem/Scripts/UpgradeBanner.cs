using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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

    private StructureLevel structureLevel;
    private StructureLevel nextLevel;
    private UpgradeManager upgradeManager;
    public UpgradeUI upgradeUI;
    private StructureType myType;


    public void SetupBanner(StructureType type, UpgradeManager manager)
    {
        myType = type;
        structureLevel = manager.GetStructureLevelOfType(type);
        nextLevel = structureLevel.NextLevel();
        upgradeManager = manager;
        UpdateBannerVisuals(structureLevel);
        upgradeButton.onClick.AddListener(PerformUpgrade);
    }
    private void UpdateBannerVisuals(StructureLevel level)
    {
        typeText.text = $"{myType.ToString()} : \n Lv {level.level} TO-> {nextLevel.level}";

        if (nextLevel != null)
        {
            costText.text = $"Mycelia: {level.upgradeCost:F1}";
            upgradeButton.interactable = true;
            descriptionText.text = nextLevel.upgradeDescription;
        }

        // Ensure button doesn't keep old listeners
        //upgradeButton.onClick.RemoveAllListeners();
    }
    void PerformUpgrade()
    {        
        float currentMycelia = GameManager.Instance.Mycelia;

        if (upgradeManager.CanUpgrade(myType, currentMycelia))
        {
            GameManager.Instance.DecreaseMycelia(structureLevel.upgradeCost);
            upgradeUI.UpdateMyceliaAmount();

            structureLevel = nextLevel;
            nextLevel = nextLevel.NextLevel();

            upgradeManager.UpgradeStructure(myType, structureLevel);
            GameManager.Instance.ApplyUpgradeToStructures(myType, structureLevel);
           
            UpdateBannerVisuals(structureLevel);
        }
    }
}
