using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsarDamage : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;

    [Header("Damage")]
    public float DamageIncreaseMultiplier = 1.25f;
    public float DamageCostIncreaseMultiplier = 1.50f;
    [SerializeField] private float damageCost = 100;
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
        banner.upgradeName.text = "Pulsar Damage";

        currentDamage = banner.upgradeMenu.activePlayer.defaultWeapon.damage;
        float newDamage = Mathf.RoundToInt(currentDamage * DamageIncreaseMultiplier);
        banner.descriptionText.text = $"Current Damage {currentDamage} -> New Damage {newDamage}";

        banner.costText.text = $"Mycelia: {damageCost.ToString("F0")}";

        banner.purchaseButton.interactable = true;
        banner.buttonText.color = Color.black;
        banner.buttonText.text = "Purchase";

        banner.upgradeMenu.UpdateMyceliaAmount();
    }
    public void UpgradePulsarDamage()
    {
        if (GameManager.Instance.Mycelia < damageCost)
        {
            banner.buttonText.color = Color.red;
            return;
        }
        int newDamage = Mathf.RoundToInt(currentDamage * DamageIncreaseMultiplier);

        foreach (PlayerManager player in GameManager.Instance.players)
        {
            Weapon pulsarPistol = player.defaultWeapon;
            pulsarPistol.damage = newDamage;
            pulsarPistol.StartReload();
        }

        

        damageCost = banner.Purchase(damageCost, DamageCostIncreaseMultiplier);
        UpdateDamageUI();
    }
    
}
