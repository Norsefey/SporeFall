using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoIndicator;
    [SerializeField] protected GameObject promptHolder;
    [SerializeField] private TMP_Text textPrompt;
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
            ammoIndicator.text = currentWeapon.bulletCount + "/" + currentWeapon.totalAmmo;
        else if(currentWeapon is BuildGun)
        {
            ammoIndicator.text = "Build Mode";
        }
        else
            ammoIndicator.text = currentWeapon.bulletCount + "/" + "\u221E";
    }
    public void UpdateHPDisplay(float value)
    {
        HPBar.value = value;
    }
    public void DisplayMycelia(float value)
    {
        ammoIndicator.text = "Mycelia: " + value.ToString();
    }
    public void EnablePrompt(string text)
    {
        promptHolder.gameObject.SetActive(true);
        textPrompt.text = text;
    }
    public void DisablePrompt()
    {
        promptHolder.gameObject.SetActive(false);
    }
}
