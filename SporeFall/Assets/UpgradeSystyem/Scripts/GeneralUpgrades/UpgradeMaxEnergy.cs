using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeMaxEnergy : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;

    [Header("Level Management")]
    public float EnergyIncreaseRate = 10;
    public float costIncreaseMultiplier = 1.50f;
    [SerializeField] private float cost = 100;

    private float currentMaxEnergy;

    private void Awake()
    {
        if (banner == null)
            banner = new UpgradeBannerUIElements();
    }
    private void OnEnable()
    {
        UpdateUIElements();
    }
    private void UpdateUIElements()
    {
        banner.upgradeName.text = "Energy Output";

        currentMaxEnergy = GameManager.Instance.maxEnergy;
        float newMaxEnergy = currentMaxEnergy + EnergyIncreaseRate;
        banner.descriptionText.text = $"Current Energy Output {currentMaxEnergy} -> New Energy Output {newMaxEnergy}";
        
        banner.costText.text = $"Mycelia: {cost.ToString("F0")}";

        banner.purchaseButton.interactable = true;
        banner.buttonText.color = Color.black;
        banner.buttonText.text = "Purchase";

        banner.upgradeMenu.UpdateMyceliaAmount();
    }
    private void Purchase()
    {
        GameManager.Instance.DecreaseMycelia(cost);
        GameManager.Instance.UpdateEnergyUsage();
        cost *= costIncreaseMultiplier;
        cost = Mathf.RoundToInt(cost);

        UpdateUIElements();
    }
    public void UpgradeEnergyCapacity()
    {
        if (GameManager.Instance.Mycelia < cost)
        {
            banner.buttonText.color = Color.red;
            return;
        }
        GameManager.Instance.maxEnergy += EnergyIncreaseRate;

        Purchase();
    }
}
