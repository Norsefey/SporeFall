using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    public GameObject upgradeBannerPrefab;
    public TMP_Text myceliaText;
    public Transform scrollViewContent;
    public GameManager gameManager;

    [SerializeField] private GameObject structBannerHolder;
    [SerializeField] private GameObject playerBannerHolder;


    private void OnEnable()
    {
        UpdateMyceliaAmount();
        ShowStructureUpgrades();
    }

    public void ShowStructureUpgrades()
    {
        playerBannerHolder.SetActive(false);
        structBannerHolder.SetActive(true);

        foreach (StructureType type in System.Enum.GetValues(typeof(StructureType)))
        {
            GameObject bannerObj = Instantiate(upgradeBannerPrefab, scrollViewContent);
            UpgradeBanner banner = bannerObj.GetComponent<UpgradeBanner>();
            banner.upgradeUI = this;
            // Fetch appropriate StructureLevels scriptable object for this type
            StructureLevels structureLevels = gameManager.upgradeManager.GetStructureLevelsForType(type);

            int currentLevel = gameManager.upgradeManager.GetStructureLevel(type);
            StructureLevel nextLevel = gameManager.upgradeManager.GetNextLevel(type);
            if(nextLevel != null)
                banner.SetupBanner(type, nextLevel);
        }
    }
    public void ShowPlayerUpgrades()
    {
        structBannerHolder.SetActive(false);
        playerBannerHolder.SetActive(true);
    }
    public void UpdateMyceliaAmount()
    {
        myceliaText.color = Color.white;

        myceliaText.text = $"Mycelia: {gameManager.Mycelia}";
        
        if(gameManager.Mycelia <= 0)
            myceliaText.color = Color.red;
    }

}
