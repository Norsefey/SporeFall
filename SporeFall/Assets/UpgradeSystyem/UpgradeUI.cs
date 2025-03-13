using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgradeManager;

    public GameObject upgradeBannerPrefab;
    public TMP_Text myceliaText;
    public Transform scrollViewContent;

    [SerializeField] private GameObject structBannerHolder;
    [SerializeField] private GameObject playerBannerHolder;
    private static readonly Color DefaultColor = Color.white;
    private static readonly Color NoMyceliaColor = Color.red;

    [SerializeField] GameObject firstUpgradeButton;
    private void Start()
    {
        UpdateMyceliaAmount();
        ShowStructureUpgrades();
    }
    public void SetSelectable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstUpgradeButton);
    }
    public void ShowStructureUpgrades()
    {

        playerBannerHolder.SetActive(false);
        structBannerHolder.SetActive(true);


        ClearBanners();

        foreach (StructureType type in System.Enum.GetValues(typeof(StructureType)))
        {
            GameObject bannerObj = Instantiate(upgradeBannerPrefab, scrollViewContent);
            UpgradeBanner banner = bannerObj.GetComponent<UpgradeBanner>();
            banner.upgradeUI = this;
            //upgradeButtons.Add(banner.upgradeButton);

            // Get structure levels and current upgrade level
            StructureLevels structureLevels = upgradeManager.GetStructureLevelsForType(type);
            int currentLevel = upgradeManager.GetStructureLevel(type);
            StructureLevel nextLevel = upgradeManager.GetNextLevel(type);

            // Always show the banner, even if it's at max level
            banner.SetupBanner(type, nextLevel ?? structureLevels.GetLevel(currentLevel), upgradeManager);
        }

        // For when it refreshes
        SetSelectable();
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
        myceliaText.color = GameManager.Instance.Mycelia > 0 ? DefaultColor : NoMyceliaColor;
        myceliaText.text = $"Mycelia: {GameManager.Instance.Mycelia}";
    }

}
