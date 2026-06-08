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
        typeText.text = $"{myType.ToString()} : \n Lv {level.level} TO-> {level.NextLevel().level}";

        costText.text = $"Mycelia: {level.upgradeCost:F1}";
        upgradeButton.interactable = true;
        descriptionText.text = level.NextLevel().upgradeDescription;
    }
    void PerformUpgrade()
    {
        float currentMycelia = GameManager.Instance.Mycelia;
        if(upgradeManager.CanUpgrade(myType, currentMycelia))
        {
            GameManager.Instance.DecreaseMycelia(structureLevel.upgradeCost);
            upgradeUI.UpdateMyceliaAmount();

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

        /*float currentMycelia = GameManager.Instance.Mycelia;
        if (upgradeManager.CanUpgrade(myType, currentMycelia))
        {
            GameManager.Instance.DecreaseMycelia(structureLevel.upgradeCost);
            upgradeUI.UpdateMyceliaAmount();

            structureLevel = structureLevel.NextLevel();

            upgradeManager.UpgradeStructure(myType, structureLevel);
            GameManager.Instance.ApplyUpgradeToStructures(myType, structureLevel);
           
            UpdateBannerVisuals(structureLevel);
        }*/
    }
}
