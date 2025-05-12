using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePlayerCorruption : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;

    [Header("Level Management")]
    public float corruptionIncreaseMultiplier = 1.25f;
    public float costIncreaseMultiplier = 1.50f;
    [SerializeField] private float cost = 50;
    private float currentMaxCorruption;

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
        banner.upgradeName.text = "Corruption Tolerance";
        currentMaxCorruption = banner.upgradeMenu.activePlayer.pCorruption.MaxCorruption;
        float newMaxCorruption = Mathf.RoundToInt(currentMaxCorruption * corruptionIncreaseMultiplier);
        banner.descriptionText.text = $"Current Max Corruption {currentMaxCorruption} -> New Max Corruption {newMaxCorruption}";

        banner.costText.text = $"Mycelia: {cost.ToString("F0")}";

        banner.purchaseButton.interactable = true;
        banner.buttonText.color = Color.black;
        banner.buttonText.text = "Purchase";

        banner.upgradeMenu.UpdateMyceliaAmount();
    }
    private void Purchase()
    {
        GameManager.Instance.DecreaseMycelia(cost);

        cost *= costIncreaseMultiplier;
        cost = Mathf.RoundToInt(cost);

        UpdateUIElements();
    }
    public void UpgradeMaxPlayerCorruption()
    {
        if (GameManager.Instance.Mycelia < cost)
        {
            banner.buttonText.color = Color.red;
            return;
        }

        float newMaxCorruption = Mathf.RoundToInt(currentMaxCorruption * corruptionIncreaseMultiplier);
        banner.upgradeMenu.activePlayer.pCorruption.SetMaxCorruption(newMaxCorruption);

        Purchase();
    }
}
