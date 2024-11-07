using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private PlayerManager pMan;
    [SerializeField] private BuildGun bGun;
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
    [Header("Build/Structures UI")]
    public GameObject buildUI;
    [SerializeField] private Image selectedStructureIcon;
    [SerializeField] private Image rightStructureIcon;
    [SerializeField] private Image leftStructureIcon;
    [SerializeField] private Sprite turretSprite;
    [SerializeField] private Sprite flamethrowerSprite;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Sprite shermanSprite;
    [SerializeField] private Sprite repairTowerSprite;
    //[SerializeField] private Sprite lilySprite;

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
        else if (currentWeapon is BuildGun)
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
        if (HPBar != null)
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

        if (toggle)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = toggle;
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }

    public void SwitchStructureIcon()
    {
        //selectedStructureIcon.sprite = bGun.selectedStructure.structureSprite;
        if (bGun.currentBuildIndex == 0)
        {
            selectedStructureIcon.sprite = turretSprite;
            leftStructureIcon.sprite = repairTowerSprite;
            rightStructureIcon.sprite = flamethrowerSprite;
        }

        if (bGun.currentBuildIndex == 1)
        {
            selectedStructureIcon.sprite = flamethrowerSprite;
            leftStructureIcon.sprite = turretSprite;
            rightStructureIcon.sprite = wallSprite;
        }

        if (bGun.currentBuildIndex == 2)
        {
            selectedStructureIcon.sprite = wallSprite;
            leftStructureIcon.sprite = flamethrowerSprite;
            rightStructureIcon.sprite = shermanSprite;
        }

        if (bGun.currentBuildIndex == 3)
        {
            selectedStructureIcon.sprite = shermanSprite;
            leftStructureIcon.sprite = wallSprite;
            rightStructureIcon.sprite = repairTowerSprite;
        }

        if (bGun.currentBuildIndex == 4)
        {
            selectedStructureIcon.sprite = repairTowerSprite;
            leftStructureIcon.sprite = shermanSprite;
            rightStructureIcon.sprite = turretSprite;
        }
    }
}
