using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeUI : MonoBehaviour
{
    public GameObject upgradeBannerPrefab;
    public TMP_Text myceliaText;
    public Transform scrollViewContent;
    public GameManager gameManager;

    [SerializeField] private GameObject structBannerHolder;
    [SerializeField] private GameObject playerBannerHolder;
    private static readonly Color DefaultColor = Color.white;
    private static readonly Color NoMyceliaColor = Color.red;

    [SerializeField] GameObject firstUpgradeButton;

    private void OnEnable()
    {
        UpdateMyceliaAmount();
        ShowStructureUpgrades();
    }

    public void ShowStructureUpgrades()
    {
        playerBannerHolder.SetActive(false);
        structBannerHolder.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstUpgradeButton);

        ClearBanners();

        foreach (StructureType type in System.Enum.GetValues(typeof(StructureType)))
        {
            GameObject bannerObj = Instantiate(upgradeBannerPrefab, scrollViewContent);
            UpgradeBanner banner = bannerObj.GetComponent<UpgradeBanner>();
            banner.upgradeUI = this;

            // Get structure levels and current upgrade level
            StructureLevels structureLevels = gameManager.upgradeManager.GetStructureLevelsForType(type);
            int currentLevel = gameManager.upgradeManager.GetStructureLevel(type);
            StructureLevel nextLevel = gameManager.upgradeManager.GetNextLevel(type);

            // Always show the banner, even if it's at max level
            banner.SetupBanner(type, nextLevel ?? structureLevels.GetLevel(currentLevel));
        }
    }
    private void ClearBanners()
    {
        // Clear existing banners before adding new ones
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
    }
    public void ShowPlayerUpgrades()
    {
        structBannerHolder.SetActive(false);
        playerBannerHolder.SetActive(true);
    }
    public void UpdateMyceliaAmount()
    {
        myceliaText.color = gameManager.Mycelia > 0 ? DefaultColor : NoMyceliaColor;
        myceliaText.text = $"Mycelia: {gameManager.Mycelia}";
    }

}
