using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PulsarBulletCapacity : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;

    /*    [Header("Bullet Capacity")]
        public float bulletCapacityIncrease = 10;
        public float bulletCostIncreaseMultiplier = 1.50f;
        [SerializeField] private float bulletCost = 50;
    */
    private float currentBulletCapacity;
    private void Awake()
    {
        if (banner == null)
            banner = GetComponent<UpgradeBannerUIElements>();
    }
    private void OnEnable()
    {
        UpdateBullectCapacityUI();
    }

    private void UpdateBullectCapacityUI()
    {
        banner.upgradeName.text = "Gun Capacity";

        if(banner.upgradeMenu.activePlayer == null)
            return;

        PlayerGunUpgrades.Instance.SetActivePlayer(banner.upgradeMenu.activePlayer);

        currentBulletCapacity = banner.upgradeMenu.activePlayer.currentWeapon.bulletCapacity;
        float newMaxCapacity = Mathf.RoundToInt(currentBulletCapacity * (1 + PlayerGunUpgrades.Instance.magazineSizeIncreasePercentage));
        banner.descriptionText.text = $"Current Bullet Capacity {currentBulletCapacity} -> New Bullet Capacity {newMaxCapacity}";

        banner.costText.text = $"Mycelia: {PlayerGunUpgrades.Instance.magazineSizeUpgradeCost.ToString("F0")}";

        banner.purchaseButton.interactable = true;
        banner.buttonText.color = Color.black;
        banner.buttonText.text = "Purchase";

        banner.upgradeMenu.UpdateMyceliaAmount();
    }
    public void UpgradePulsarMaxCapacity()
    {
        if (GameManager.Instance.Mycelia < PlayerGunUpgrades.Instance.magazineSizeUpgradeCost)
        {
            banner.buttonText.color = Color.red;
            return;
        }
        GameManager.Instance.DecreaseMycelia(PlayerGunUpgrades.Instance.magazineSizeUpgradeCost);
        // new cost is added so needs to be after the cost is taken from the player
        PlayerGunUpgrades.Instance.UpgradeMagazineSize(banner.upgradeMenu.activePlayer.currentWeapon);

        if (banner.upgradeMenu.activePlayer.currentWeapon != banner.upgradeMenu.activePlayer.defaultWeapon)
        {
            PlayerGunUpgrades.Instance.UpgradeMagazineSize(banner.upgradeMenu.activePlayer.defaultWeapon);
        }

        UpdateBullectCapacityUI();
    }
}
