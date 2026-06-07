using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PulsarFireRate : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;
    /*
        [Header("Fire Rate")]
        public float fireRateIncreaseMultiplier = 1.25f;
        public float fireRateCostIncreaseMultiplier = 1.50f;
        [SerializeField] private float fireRateCost = 85;
    */
    private float currentFireRate;
    private void Awake()
    {
        if (banner == null)
            banner = GetComponent<UpgradeBannerUIElements>();
    }
    private void OnEnable()
    {
        UpdateFireRateUI();
    }
    public void UpdateFireRateUI()
    {
        banner.upgradeName.text = "Weapon Fire Rate";
        if (banner.upgradeMenu.activePlayer == null)
            return;
        PlayerGunUpgrades.Instance.SetActivePlayer(banner.upgradeMenu.activePlayer);

        currentFireRate = banner.upgradeMenu.activePlayer.currentWeapon.fireRate;
        float newFR = Mathf.RoundToInt(currentFireRate * (1 + PlayerGunUpgrades.Instance.fireRateIncreasePercentage));
        banner.descriptionText.text = $"Current Fire Rate {currentFireRate} -> New Fire Rate {newFR}";


        banner.costText.text = $"Mycelia: {PlayerGunUpgrades.Instance.fireRateUpgradeCost.ToString("F0")}";
        banner.purchaseButton.interactable = true;
        banner.buttonText.color = Color.black;
        banner.buttonText.text = "Purchase";

        banner.upgradeMenu.UpdateMyceliaAmount();
    }

    public void UpgradePulsarFireRate()
    {
        if (GameManager.Instance.Mycelia < PlayerGunUpgrades.Instance.fireRateUpgradeCost)
        {
            banner.buttonText.color = Color.red;
            return;
        }
        GameManager.Instance.DecreaseMycelia(PlayerGunUpgrades.Instance.fireRateUpgradeCost);
        // new cost is added so needs to be after the cost is taken from the player
        PlayerGunUpgrades.Instance.UpgradeFireRate(banner.upgradeMenu.activePlayer.currentWeapon);
        
        if (banner.upgradeMenu.activePlayer.currentWeapon != banner.upgradeMenu.activePlayer.defaultWeapon)
        {
            PlayerGunUpgrades.Instance.UpgradeFireRate(banner.upgradeMenu.activePlayer.defaultWeapon);
        }

        UpdateFireRateUI();
    }
}
