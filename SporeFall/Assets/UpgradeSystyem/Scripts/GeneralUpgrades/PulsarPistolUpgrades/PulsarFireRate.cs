using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PulsarFireRate : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;

    [Header("Fire Rate")]
    public float fireRateIncreaseMultiplier = 1.25f;
    public float fireRateCostIncreaseMultiplier = 1.50f;
    [SerializeField] private float fireRateCost = 85;
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
        banner.upgradeName.text = "Pulsar Fire Rate";

        AutomaticGun pulsarPistol = (AutomaticGun)banner.upgradeMenu.activePlayer.defaultWeapon;

        currentFireRate = pulsarPistol.fireRate;
        float newFR = Mathf.RoundToInt(currentFireRate * fireRateIncreaseMultiplier);
        banner.descriptionText.text = $"Current Fire Rate {currentFireRate} -> New Fire Rate {newFR}";


        banner.costText.text = $"Mycelia: {fireRateCost.ToString("F0")}";
        banner.purchaseButton.interactable = true;
        banner.buttonText.color = Color.black;
        banner.buttonText.text = "Purchase";

        banner.upgradeMenu.UpdateMyceliaAmount();
    }

    public void UpgradePulsarFireRate()
    {
        if (GameManager.Instance.Mycelia < fireRateCost)
        {
            banner.buttonText.color = Color.red;
            return;
        }

        float newFR = Mathf.RoundToInt(currentFireRate * fireRateIncreaseMultiplier);

        foreach (PlayerManager player in GameManager.Instance.players)
        {
            AutomaticGun pulsarPistol = (AutomaticGun)player.defaultWeapon;
            pulsarPistol.fireRate = newFR;
        }
        fireRateCost = banner.Purchase(fireRateCost, fireRateCostIncreaseMultiplier);
        UpdateFireRateUI();
    }
}
