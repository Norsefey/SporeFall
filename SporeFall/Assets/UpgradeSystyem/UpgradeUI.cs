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
    private void OnEnable()
    {
        UpdateMyceliaAmount();
        PopulateUpgradeBanners();
    }

    public void PopulateUpgradeBanners()
    {
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

    public void UpdateMyceliaAmount()
    {
        myceliaText.color = Color.white;

        myceliaText.text = $"Mycelia: {gameManager.Mycelia}";
        
        if(gameManager.Mycelia <= 0)
            myceliaText.color = Color.red;
    }

}
