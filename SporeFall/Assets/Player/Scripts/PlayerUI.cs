using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]TMP_Text ammoIndicator;

    public void AmmoDisplay(Weapon currentWeapon)
    {
        if(currentWeapon.limitedAmmo)
            ammoIndicator.text = currentWeapon.magazineCount + "/" + currentWeapon.totalAmmo;
        else if(currentWeapon is BuildGun gun)
        {
            ammoIndicator.text = gun.SelectedStructure();
        }
        else
            ammoIndicator.text = currentWeapon.magazineCount + "/" + "\u221E";
    }
}
