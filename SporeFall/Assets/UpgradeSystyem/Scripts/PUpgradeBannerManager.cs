using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PUpgradeBannerManager : MonoBehaviour
{
    public enum PBannerType
    {
        Energy,
        PlayerHP,
        PulsarCapacity,
        PulsarFireRate,
        PulsarDamage,
        PulsarRange
    }

    public PBannerType type;

    [Header("References")]
    public UpgradeUI upgradeUI;

    [Header("Level Management")]
    public int currentLevel = 0;
    public float costIncreaseMultiplier = 1.50f;
    [SerializeField] private float cost = 50;

    [Header("UI Elements")]
    [SerializeField] private ButtonTextMovement textMove;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text buttonText;
    public TMP_Text upgradeTypeText;
    public TMP_Text costText;
    public TMP_Text descriptionText;


    private void OnEnable()
    {
        purchaseButton.interactable = true;
        buttonText.color = Color.black;
        buttonText.text = "Purchase";
    }

    private void UpdateUIElements()
    {
        costText.text = $"Mycelia: {cost.ToString("F0")}";
        
        purchaseButton.interactable = true;
        buttonText.color = Color.black;
        buttonText.text = "Purchase";

        descriptionText.text = UpgradeDescription();
    }
    private void Purchase()
    {
        GameManager.Instance.DecreaseMycelia(cost);

        currentLevel++;
        cost *= costIncreaseMultiplier;
        cost = Mathf.RoundToInt(cost);

        UpdateUIElements();
    }
    private string UpgradeDescription()
    {
        string description = "";



        return description;
    }
    public void UpgradeEnergyCapacity()
    {
        if (GameManager.Instance.Mycelia < cost)
        {
            buttonText.color = Color.red;
            return;
        }
    }
    public void UpgradeMaxPlayerHP()
    {
        if (GameManager.Instance.Mycelia < cost)
        {
            buttonText.color = Color.red;
            return;
        }
    }
    public void UpgradeMaxCorruption()
    {

    }
    public void UpgradePulsarMaxCapacity()
    {
        if (GameManager.Instance.Mycelia < cost)
        {
            buttonText.color = Color.red;
            return;
        }
    }
    public void UpgradePulsarDamage()
    {
        if (GameManager.Instance.Mycelia < cost)
        {
            buttonText.color = Color.red;
            return;
        }
    }
    public void UpgradePulsarFireRate()
    {
        if (GameManager.Instance.Mycelia < cost)
        {
            buttonText.color = Color.red;
            return;
        }
    }
    public void UpgradePulsarRange()
    {
        if (GameManager.Instance.Mycelia < cost)
        {
            buttonText.color = Color.red;
            return;
        }
    }
}
