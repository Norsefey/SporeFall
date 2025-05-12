using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBannerUIElements : MonoBehaviour
{
    [Header("References")]
    public UpgradeUI upgradeMenu;

    [Header("UI Elements")]
    public ButtonTextMovement textMove;
    public Button purchaseButton;
    public TMP_Text buttonText;
    public TMP_Text upgradeName;
    public TMP_Text costText;
    public TMP_Text descriptionText;
}
