using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePlayerHP : MonoBehaviour
{
    [SerializeField] private UpgradeBannerUIElements banner;

    [Header("Level Management")]
    public float HPIncreaseMultiplier = 1.5f;
    public float costIncreaseMultiplier = 1.50f;
    [SerializeField] private float cost = 50;
    private float currentMaxHP;


    private void Awake()
    {
        if(banner == null)
            banner = GetComponent<UpgradeBannerUIElements>();
    }
    private void OnEnable()
    {
        UpdateUIElements();
    }
    private void UpdateUIElements()
    {
        banner.upgradeName.text = "Aegis HP";

        currentMaxHP = banner.upgradeMenu.activePlayer.pHealth.MaxHP;
        float newMaxHP = Mathf.RoundToInt(currentMaxHP * HPIncreaseMultiplier);
        banner.descriptionText.text = $"Current Max HP {currentMaxHP} -> New Max HP {newMaxHP}";

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
    public void UpgradeMaxPlayerHP()
    {
        if (GameManager.Instance.Mycelia < cost)
        {
            banner.buttonText.color = Color.red;
            return;
        }

        float newMaxHP = Mathf.RoundToInt(currentMaxHP * HPIncreaseMultiplier);
        banner.upgradeMenu.activePlayer.pHealth.SetMaxHP(newMaxHP);

        Purchase();
    }
}
