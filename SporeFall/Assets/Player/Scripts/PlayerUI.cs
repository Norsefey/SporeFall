using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private PlayerManager pMan;
    private Weapon weapon;
    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameplayUI;
    [Space(5)]
    [SerializeField] private TMP_Text ammoIndicator;
    [SerializeField] private GameObject promptHolder;
    [SerializeField] private TMP_Text textPrompt;
    [SerializeField] private Slider corruptionBar;
    [SerializeField] private Slider HPBar;
    [Header("Upgrade Menu")]
    [SerializeField] private GameObject upgradeMenu;
    //[Header("Weapon Icons")]
    //[SerializeField] private GameObject puffBoomIcon;
    //[SerializeField] private GameObject rifleIcon;
    //[SerializeField] private GameObject shotgunIcon;

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
