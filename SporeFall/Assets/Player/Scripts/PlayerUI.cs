using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private PlayerManager pMan;
    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameplayUI;
    public Image weaponIcon;
    //public Sprite weaponSprite;
    [Space(5)]
    [SerializeField] private TMP_Text ammoIndicator;
    [SerializeField] private GameObject promptHolder;
    [SerializeField] private TMP_Text textPrompt;
    [SerializeField] private Slider corruptionBar;
    [SerializeField] private Slider HPBar;
    public GameObject life1;
    public GameObject life2;
    [Header("Upgrade Menu")]
    [SerializeField] private GameObject upgradeMenu;

    private void Start()
    {
        corruptionBar.maxValue = pMan.pCorruption.maxCorruption;
        HPBar.maxValue = pMan.pHealth.maxHP;
    }
    public void DisplayCorruption(float value)
    {
        if (corruptionBar != null)
        {
            corruptionBar.value = value;
        }
    }
    public void AmmoDisplay(Weapon currentWeapon)
    {
        if (currentWeapon.IsReloading)
            ammoIndicator.text = "Reloading";    
        else if (currentWeapon.limitedAmmo)
            ammoIndicator.text = currentWeapon.bulletCount + "/" + currentWeapon.totalAmmo;
        else if(currentWeapon is BuildGun)
            ammoIndicator.text = "Build Mode";
        else
            ammoIndicator.text = currentWeapon.bulletCount + "/" + "\u221E";
    }
    public void SwitchWeaponIcon()
    {
        weaponIcon.sprite = pMan.currentWeapon.weaponSprite;
    }
    public void UpdateHPDisplay(float value)
    {
        if(HPBar != null)
            HPBar.value = value;
    }
    public void DisplayMycelia(float value)
    {
        ammoIndicator.text = "Mycelia: " + value.ToString();
    }
    public void EnablePrompt(string text)
    {
        promptHolder.SetActive(true);
        textPrompt.text = text;
    }
    public void DisablePrompt()
    {
        promptHolder.SetActive(false);
    }
    public void ToggleUpgradeMenu(bool toggle)
    {
        gameplayUI.SetActive(!toggle);
        upgradeMenu.SetActive(toggle);
        upgradeMenu.GetComponent<UpgradeMenu>().SetupMenu(pMan);

        pMan.TogglePControl(!toggle);

        if(toggle)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = toggle;
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
