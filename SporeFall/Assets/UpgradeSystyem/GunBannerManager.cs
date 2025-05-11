using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunBannerManager : MonoBehaviour
{
    [Header("References")]
    public UpgradeUI upgradeUI;

    [Header("Gun Settings")]
    public GameObject gunForSale;
    [SerializeField] private float cost;

    [Header("UI Elements")]
    [SerializeField] private ButtonTextMovement textMove;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text buttonText;
    public TMP_Text gunName;
    public TMP_Text costText;
    public TMP_Text descriptionText;

    private void Awake()
    {
        costText.text = $"Mycelia: {cost}";
        gunName.text = gunForSale.GetComponent<Weapon>().weaponName;
    }

    private void OnEnable()
    {
        purchaseButton.interactable = true;
        buttonText.color = Color.black;
        buttonText.text = "Purchase";
    }

    public void PurchaseWeapon() 
    { 
        if(GameManager.Instance.Mycelia < cost)
        {
            buttonText.color = Color.red;
            return;
        }

        GameManager.Instance.DecreaseMycelia(cost);
        upgradeUI.UpdateMyceliaAmount();
        upgradeUI.activePlayer.EquipNewGun(gunForSale);

        purchaseButton.interactable = false;
        buttonText.color = Color.black;
        buttonText.text = "Bought";
    }
}
