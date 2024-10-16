using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeBanner : MonoBehaviour
{
    [Header("Display Info")]
    [SerializeField] private TMP_Text upgradeName;
    [SerializeField] private TMP_Text statChange;
    [SerializeField] private TMP_Text upgradeCost;

    public void SetDisplay(Upgrade upgrade)
    {
        upgradeName.text = upgrade.upgradeName;
        upgradeCost.text = upgrade.cost.ToString();
        statChange.text = upgrade.currentValue + "->" + upgrade.upgradeAmount;
    }

    public void PurchaseUpgrade()
    {
        Debug.Log("Bought Upgrade");
    }
}
