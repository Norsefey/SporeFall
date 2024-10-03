// Ignore Spelling: mycelia

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement pController;
    public PlayerInputOrganizer pInput;
    public TPSCamera pCamera;
    public PlayerUI pUI;
    public GameObject pVisual;
    public BuildGun bGun;
    public TrainHandler train;
    public WaveManager waveManager;
    [Header("Weapons")]
    // Weapons and Shooting
    public Transform weaponHolder; // Where the weapon is equipped
    public Weapon currentWeapon;
    public Weapon defaultWeapon;
    public Weapon equippedWeapon;
    public GameObject nearByWeapon;
    public bool isFiring = false;
    public bool isCharging = false;
    public bool isBuilding = false;
    public bool isRotating = false;

    // Player Stats
    public int lives = 3;
    public float HP = 100;
    public float mycelia = 30;
    [Header("Corruption Stuff")]
    public float corruptionLevel = 0;
    public float purifyRate = 1;
    private bool holdingCorrupted = false;
    private void Awake()
    {
        pInput = GetComponent<PlayerInputOrganizer>();
        // since we will have multiple players, this manager cannot be a public instance, so we assign it locally
        pController.SetManager(this);
        pCamera.SetManager(this);
        pInput.SetManager(this);

        PlayerInput playerInput = GetComponent<PlayerInput>();

        if(playerInput.devices.Count > 0)
        {
            var device = playerInput.devices[0];

            if(device is Gamepad)
            {
                Debug.Log("I am using a gamepad");
                pCamera.SetGamepadSettings();
                bGun.structRotSpeed = 50;
            }
            else if (device is Keyboard)
            {
                Debug.Log("I am using a keyboard");
                pCamera.SetMouseSettings();
                bGun.structRotSpeed = 25;
            }
        }
    }
    private void Start()
    {
        // in order to spawn player at a spawn point, disable movement controls
        pUI.AmmoDisplay(currentWeapon);
        pInput.AssignAllActions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        if (currentWeapon != null)
        {
            if (holdingCorrupted)
            {
                corruptionLevel += currentWeapon.corruptionRate * Time.deltaTime;
                pUI.DisplayCorruption(corruptionLevel);
            }else if(corruptionLevel > 0)
            {
                corruptionLevel -= Time.deltaTime * purifyRate;
                pUI.DisplayCorruption(corruptionLevel);
            }

            if (currentWeapon is BuildGun bGun)
            {
                pUI.DisplayMycelia(mycelia);
                if (bGun.isEditing)
                {
                    bGun.RotateStructure();
                }
            }
           
            if (isFiring && !currentWeapon.IsReloading && currentWeapon is not ChargeGun)
            {
                currentWeapon.Fire();
                pUI.AmmoDisplay(currentWeapon);
            }else if (isCharging && (currentWeapon is ChargeGun gun))
            {
                // Charge weapons handle firing when the fire button is held
                gun.Charge();
            }
        }
    } 
    public void PromptPickUpWeapon(GameObject weapon)
    {
        pInput.AssignWeaponPickUp();
        nearByWeapon = weapon;
        pUI.EnablePrompt("Press F to Pick up: " + weapon.name);
    }
    public void DisablePickUpWeaponPrompt() 
    {
        pInput.RemoveWeaponPickUp();
        nearByWeapon = null;
        pUI.DisablePrompt();
    }
    public void SetBuildMode()
    {
        if (!isBuilding)
        {// Enter Build mode
            currentWeapon.gameObject.SetActive(false);
            bGun.gameObject.SetActive(true);
            currentWeapon = bGun;
            pUI.EnablePrompt("Use Q/E to change Structure" + "\n F to Select Structure" + "\n Hold Right mouse to Preview");
            pUI.AmmoDisplay(currentWeapon);
            isBuilding = true;
        }
        else
        {// Exit Build Mode
            if (equippedWeapon != null)
                currentWeapon = equippedWeapon;
            else
                currentWeapon = defaultWeapon;
            if(bGun.isEditing)
                bGun.DeSelectStructure();
            else
                bGun.DestroySelectedObject();

            currentWeapon.gameObject.SetActive(true);
            pUI.AmmoDisplay(currentWeapon);
            pUI.DisablePrompt();
            bGun.gameObject.SetActive(false);
            isBuilding = false;
        }
    }
    public void PickUpWeapon()
    {
        if (nearByWeapon == null)
            return;
        // deactivate the default weapon
        if (currentWeapon == defaultWeapon || currentWeapon == bGun)
        {
            currentWeapon.gameObject.SetActive(false);
        }else if(equippedWeapon != null)
        {
            Destroy(currentWeapon.gameObject); // Drop the current weapon
        }
        // Equip the new weapon
        currentWeapon = Instantiate(nearByWeapon, weaponHolder).GetComponent<Weapon>();
        // set the transforms of the new weapon
        currentWeapon.transform.forward = pController.transform.forward;
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.player = this;
        equippedWeapon = currentWeapon;
        // destroy pick up platform
        Destroy(nearByWeapon.transform.parent.gameObject);
        // disable pick up prompt
        pUI.DisablePrompt();
        // update UI to display new ammo capacities
        pUI.AmmoDisplay(currentWeapon);
        Debug.Log("Picked up: " + currentWeapon.weaponName);

        if(currentWeapon.isCorrupted)
            holdingCorrupted = true;
    }
    public void DropWeapon()
    {
        if(equippedWeapon == null || currentWeapon == bGun)
            return;
        Debug.Log("Dropping Weapon");
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject); // Drop the current weapon
        }

        currentWeapon = defaultWeapon;
        equippedWeapon = null;
        // reactivate the default weapon
        currentWeapon.gameObject.SetActive(true);
        pUI.AmmoDisplay(currentWeapon);
        holdingCorrupted = false;
    }
    public void DisableControl()
    {
        pVisual.SetActive(false);
        pController.gameObject.SetActive(false);
        pCamera.gameObject.SetActive(false);
        pController.transform.localPosition = Vector3.zero;

        pInput.DisableAllInputs();
    }
    public void EnableControl()
    {
        pVisual.SetActive(true);
        pController.gameObject.SetActive(true);
        pCamera.gameObject.SetActive(true);

        pInput.EnableDefaultInputs();
    }
    public void AssignButtonAction()
    {
        pInput.AssignButtonPush();

        WaveManager.WavePhase currentPhase = waveManager.wavePhase;

        switch (currentPhase)
        {
            case WaveManager.WavePhase.NotStarted:
                pUI.EnablePrompt("Press F to Start Wave");
                break;
            case WaveManager.WavePhase.Departing:
                pUI.EnablePrompt("Press F to go to next Area");
                break;
        }
    }
    public void RemoveButtonAction()
    {
        pInput.RemoveButtonPush();
        pUI.DisablePrompt();
    }
    public void OnButtonPush()
    {
        WaveManager.WavePhase currentPhase = waveManager.wavePhase;
        switch (currentPhase)
        {
            case WaveManager.WavePhase.NotStarted:
                waveManager.OnStartWave();
                pUI.DisablePrompt();
                break;
            case WaveManager.WavePhase.Departing:
                waveManager.SkipDepartTime();
                pUI.DisablePrompt();
                break;
        }
    }
}
