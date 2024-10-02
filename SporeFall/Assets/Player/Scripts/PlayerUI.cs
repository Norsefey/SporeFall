using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoIndicator;
    [SerializeField] private TMP_Text pickUpPrompt;
    [SerializeField] private Slider corruptionBar;
    [SerializeField] private Slider HPBar;
    public void DisplayCorruption(float value)
    {
        if (corruptionBar != null)
        {
            corruptionBar.value = value;
        }
    }
    public void AmmoDisplay(Weapon currentWeapon)
    {
        if(currentWeapon.limitedAmmo)
            ammoIndicator.text = currentWeapon.magazineCount + "/" + currentWeapon.totalAmmo;
        else if(currentWeapon is BuildGun)
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
