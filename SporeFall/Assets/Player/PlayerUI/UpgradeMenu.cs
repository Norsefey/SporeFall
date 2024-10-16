using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [SerializeField] private RectTransform contentDisplay;
    [SerializeField] private GameObject bannerPrefab;
    [SerializeField] private TMP_Text myceliaAmount;
    private PlayerManager player;

    [SerializeField] private Upgrade[] playerUpgrades;
    [SerializeField] private Upgrade[] gunUpgrades;
    [SerializeField] private Upgrade[] buildUpgrades;
    [SerializeField] private Upgrade[] trainUpgrades;


    public void SetupMenu(PlayerManager player)
    {
        this.player = player;
        myceliaAmount.text = "Mycelia: \n" + player.mycelia.ToString();

        DisplayPlayerUpgrades();
    }
    public void DisplayPlayerUpgrades()
    {
        ClearContent();
        foreach (Upgrade upgrade in playerUpgrades)
        {
            UpgradeBanner banner = Instantiate(bannerPrefab, contentDisplay).GetComponent<UpgradeBanner>();
            banner.SetDisplay(upgrade);
        }
    }
    public void DisplayTrainUpgrades()
    {
        ClearContent();
        foreach (Upgrade upgrade in trainUpgrades)
        {
            UpgradeBanner banner = Instantiate(bannerPrefab, contentDisplay).GetComponent<UpgradeBanner>();
            banner.SetDisplay(upgrade);
        }
    }
    public void DisplayGunUpgrades()
    {
        ClearContent();
        foreach (Upgrade upgrade in gunUpgrades)
        {
            UpgradeBanner banner = Instantiate(bannerPrefab, contentDisplay).GetComponent<UpgradeBanner>();
            banner.SetDisplay(upgrade);
        }
    }
    public void DisplayBuildUpgrades()
    {
        ClearContent();
        foreach (Upgrade upgrade in buildUpgrades)
        {
            UpgradeBanner banner = Instantiate(bannerPrefab, contentDisplay).GetComponent<UpgradeBanner>();
            banner.SetDisplay(upgrade);
        }
    }
    public void CloseMenu()
    {
        ClearContent();
        player.pUI.ToggleUpgradeMenu(false);
    }
    private void ClearContent()
    {
        if (contentDisplay.transform.childCount > 0)
        {
            foreach (Transform child in contentDisplay.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
