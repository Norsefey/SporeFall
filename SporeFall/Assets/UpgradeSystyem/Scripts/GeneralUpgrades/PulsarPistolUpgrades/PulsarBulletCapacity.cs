using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PulsarBulletCapacity : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;

    [Header("Bullet Capacity")]
    public float bulletCapacityIncrease = 10;
    public float bulletCostIncreaseMultiplier = 1.50f;
    [SerializeField] private float bulletCost = 50;
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
        banner.upgradeName.text = "Pulsar Capacity";

        currentBulletCapacity = banner.upgradeMenu.activePlayer.defaultWeapon.bulletCapacity;
        float newMaxCapacity = Mathf.RoundToInt(currentBulletCapacity + bulletCapacityIncrease);
        banner.descriptionText.text = $"Current Bullet Capacity {currentBulletCapacity} -> New Bullet Capacity {newMaxCapacity}";

        banner.costText.text = $"Mycelia: {bulletCost.ToString("F0")}";

        banner.purchaseButton.interactable = true;
        banner.buttonText.color = Color.black;
        banner.buttonText.text = "Purchase";

        banner.upgradeMenu.UpdateMyceliaAmount();
    }
    public void UpgradePulsarMaxCapacity()
    {
        if (GameManager.Instance.Mycelia < bulletCost)
        {
            banner.buttonText.color = Color.red;
            return;
        }
        int newCapacity = Mathf.RoundToInt(currentBulletCapacity + bulletCapacityIncrease);

        Weapon pulsarPistol = banner.upgradeMenu.activePlayer.defaultWeapon;
        pulsarPistol.bulletCapacity = newCapacity;
        pulsarPistol.StartReload();

        bulletCost = banner.Purchase(bulletCost, bulletCostIncreaseMultiplier);
        UpdateBullectCapacityUI();
    }
}
