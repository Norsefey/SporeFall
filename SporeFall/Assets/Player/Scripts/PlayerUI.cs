using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]TMP_Text ammoIndicator;

    public void AmmoDisplay(int magazineCount, int totalAmmo)
    {
        ammoIndicator.text = magazineCount + "/" + totalAmmo;
    }
}
