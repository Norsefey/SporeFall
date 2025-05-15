using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PulsarRange : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;

    [Header("Range")]
    public float rangeIncreaseMultiplier = 10;
    public float rangeCostIncreaseMultiplier = 1.50f;
    [SerializeField] private float rangeCost = 75;
    private float currentRange;

    private void Awake()
    {
        if (banner == null)
            banner = GetComponent<UpgradeBannerUIElements>();
    }

    private void OnEnable()
    {
        UpdateRangeUI();
    }

    private void UpdateRangeUI()
    {
        banner.upgradeName.text = "Pulsar Range";

        currentRange = banner.upgradeMenu.activePlayer.defaultWeapon.hitScanDistance;
        float newRange = Mathf.RoundToInt(currentRange + rangeIncreaseMultiplier);
        banner.descriptionText.text = $"Current Range {currentRange} -> New Range {newRange}";


        banner.costText.text = $"Mycelia: {rangeCost.ToString("F0")}";
        banner.purchaseButton.interactable = true;
        banner.buttonText.color = Color.black;
        banner.buttonText.text = "Purchase";

        banner.upgradeMenu.UpdateMyceliaAmount();
    }
    public void UpgradePulsarRange()
    {
        if (GameManager.Instance.Mycelia < rangeCost)
        {
            banner.buttonText.color = Color.red;
            return;
        }

        float newRange = Mathf.RoundToInt(currentRange + rangeIncreaseMultiplier);
        foreach (PlayerManager player in GameManager.Instance.players)
        {
            player.defaultWeapon.hitScanDistance = newRange;

        }
        rangeCost = banner.Purchase(rangeCost, rangeCostIncreaseMultiplier);
        UpdateRangeUI();
    }
}
