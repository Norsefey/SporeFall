using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsarDamage : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;
/*
    [Header("Damage")]
    public float DamageIncreaseMultiplier = 1.25f;
    public float DamageCostIncreaseMultiplier = 1.50f;
    [SerializeField] private float damageCost = 100;*/
    private float currentDamage;
    private void Awake()
    {
        if (banner == null)
            banner = GetComponent<UpgradeBannerUIElements>();
    }
    private void OnEnable()
    {
        UpdateDamageUI();
    }

    private void UpdateDamageUI()
    {
        banner.upgradeName.text = "Weapon Damage";

        if (banner.upgradeMenu.activePlayer == null)
            return;

        PlayerGunUpgrades.Instance.SetActivePlayer(banner.upgradeMenu.activePlayer);

        currentDamage = banner.upgradeMenu.activePlayer.currentWeapon.damage;
        float newDamage = Mathf.RoundToInt(currentDamage * (1 + PlayerGunUpgrades.Instance.damageIncreasePercentage));
        banner.descriptionText.text = $"Current Damage {currentDamage.ToString("F0")} -> New Damage {newDamage.ToString("F0")}";

        banner.costText.text = $"Mycelia: {PlayerGunUpgrades.Instance.damageUpgradeCost.ToString("F0")}";

        banner.purchaseButton.interactable = true;
        banner.buttonText.color = Color.black;
        banner.buttonText.text = "Purchase";

        banner.upgradeMenu.UpdateMyceliaAmount();
    }
    public void UpgradePulsarDamage()
    {
        if (GameManager.Instance.Mycelia < PlayerGunUpgrades.Instance.damageUpgradeCost)
        {
            banner.buttonText.color = Color.red;
            return;
        }
        GameManager.Instance.DecreaseMycelia(PlayerGunUpgrades.Instance.damageUpgradeCost);
        // new cost is added so needs to be after the cost is taken from the player
        PlayerGunUpgrades.Instance.UpgradeDamage();
        UpdateDamageUI();
    }
    
}
