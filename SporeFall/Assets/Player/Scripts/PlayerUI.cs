using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]TMP_Text ammoIndicator;
    [SerializeField] TMP_Text pickUpPrompt;
    public void AmmoDisplay(Weapon currentWeapon)
    {
        if(currentWeapon.limitedAmmo)
            ammoIndicator.text = currentWeapon.magazineCount + "/" + currentWeapon.totalAmmo;
        else if(currentWeapon is BuildGun gun)
        {
            ammoIndicator.text = "Build Mode";
        }
        else
            ammoIndicator.text = currentWeapon.magazineCount + "/" + "\u221E";
    }

    public void EnablePrompt(string text)
    {
        pickUpPrompt.gameObject.SetActive(true);
        pickUpPrompt.text = text;
    }
    public void DisablePrompt()
    {
        pickUpPrompt.gameObject.SetActive(false);
    }
}
