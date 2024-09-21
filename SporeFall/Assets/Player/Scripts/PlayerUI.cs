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
            ammoIndicator.text = gun.SelectedStructure();
        }
        else
            ammoIndicator.text = currentWeapon.magazineCount + "/" + "\u221E";
    }
    public void EnablePromptPickUp(GameObject weaponName)
    {
        pickUpPrompt.gameObject.SetActive(true);
        pickUpPrompt.text = "Press F To Pick Up: " + "\n" + weaponName.name;
    }
    public void DisablePromptPickUp()
    {
        pickUpPrompt.gameObject.SetActive(false);
    }
}
