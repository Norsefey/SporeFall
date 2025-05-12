using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeUI : MonoBehaviour
{
    private UpgradeManager upgradeManager;
    public GameObject upgradeBannerPrefab;
    public TMP_Text myceliaText;
    public Transform scrollViewContent;
    private static readonly Color DefaultColor = Color.white;
    private static readonly Color NoMyceliaColor = Color.red;
    [SerializeField] GameObject firstUpgradeButton;
    public PlayerManager activePlayer;

    [Header("Banner Holders")]
    [SerializeField] private GameObject structBannerHolder;
    [SerializeField] private GameObject gunsBannerHolder;
    [SerializeField] private GameObject generalBannerHolder;

    private void OnEnable()
    {
        upgradeManager = GameManager.Instance.upgradeManager;
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
        HideBannerHolders();
        structBannerHolder.SetActive(true);
        ClearUpgradeBanners();

        foreach(GameObject structureObj in GameManager.Instance.availableStructures)
        {
            GameObject bannerObj = Instantiate(upgradeBannerPrefab, scrollViewContent);
            UpgradeBanner banner = bannerObj.GetComponent<UpgradeBanner>();
            banner.upgradeUI = this;
            // Get structure levels and current upgrade level
            Structure structure = structureObj.GetComponent<Structure>();
            StructureLevels structureLevels = upgradeManager.GetStructureLevelsForType(structure.GetStructureType());
            int currentLevel = upgradeManager.GetStructureLevel(structure.GetStructureType());
            StructureLevel nextLevel = upgradeManager.GetNextLevel(structure.GetStructureType());
            // Always show the banner, even if it's at max level
            banner.SetupBanner(structure.GetStructureType(), nextLevel ?? structureLevels.GetLevel(currentLevel), upgradeManager);
        }

        // For when it refreshes
        SetSelectable();
        UpdateMyceliaAmount();
    }
    public void ShowGunStore()
    {
        HideBannerHolders();
        gunsBannerHolder.SetActive(true);
        Debug.Log("Showing Gun Store");

    }
    public void ShowGeneralStore()
    {
        HideBannerHolders();
        generalBannerHolder.SetActive(true);
    }
    private void ClearUpgradeBanners()
    {
        // Clear existing banners before adding new ones
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
    }
    private void HideBannerHolders()
    {
        if (gunsBannerHolder != null)
            gunsBannerHolder.SetActive(false);
        if (generalBannerHolder != null)
            generalBannerHolder.SetActive(false);
        if(structBannerHolder != null)
            structBannerHolder.SetActive(false);

    }
 
    public void UpdateMyceliaAmount()
    {
        myceliaText.color = GameManager.Instance.Mycelia > 0 ? DefaultColor : NoMyceliaColor;
        myceliaText.text = $"Mycelia: {GameManager.Instance.Mycelia}";
    }
    public void CloseUpgradeMenu()
    {
        activePlayer.pInput.ToggleUpgradeMenu(false);
    }
}
