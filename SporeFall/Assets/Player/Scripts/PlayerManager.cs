// Ignore Spelling: mycelia Interactable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement pController;
    public PlayerInputOrganizer pInput;
    public TPSCamera pCamera;
    public PlayerHP pHealth;
    public PlayerUI pUI;
    public GameObject pVisual;
    public BuildGun bGun;
    public PlayerAnimation pAnime;
    public CorruptionHandler pCorruption;
    public Interactables interactable;
    [Header("Default Weapons")]
    public Weapon defaultWeapon;
    public Weapon defaultSword;

    // Weapons and Shooting
    [Header("Weapons")]
    public Transform weaponHolder; // Where the weapon is equipped
    public Weapon currentWeapon;
    public Weapon equippedWeapon;
    // pickables
    [HideInInspector]
    public GameObject nearByWeapon;

    public bool isFiring = false;
    public bool isCharging = false;
    public bool isBuilding = false;
    public bool isRotating = false;
    [Header("Currency")]
    [Header("Respawn")]
    [SerializeField] private float respawnTime;
    [SerializeField] private Transform fallbackSpawnPoint;

    public bool holdingCorruption = false;
    public InputDevice myDevice;
    private bool meleeActive = false;
    private void Awake()
    {
        pInput = GetComponent<PlayerInputOrganizer>();
        // since we will have multiple players, this manager cannot be a public instance, so we assign it locally
        SetManager();
        SetDeviceSettings();
        if(Tutorial.Instance != null)
            Tutorial.Instance.playerActive = true;
    }
    private void Start()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.HandlePlayerJoining(this);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pInput.AssignAllActions();

    }
    private void Update()
    {
        WeaponBehavior();


        // For Testing
        {
            if (Input.GetKeyDown(KeyCode.Y) && !isBuilding)
            {
                if(defaultSword == null)
                    return;

                if (!meleeActive)
                {
                    meleeActive = true;

                    currentWeapon.gameObject.SetActive(false);

                    currentWeapon = defaultSword;
                    currentWeapon.gameObject.SetActive(true);
                    pAnime.SetWeaponHoldAnimation(currentWeapon.holdType);
                    pUI.ToggleDefaultUI(false);
                }
                else
                {
                    meleeActive = false;

                    currentWeapon.gameObject.SetActive(false);

                    EquipDefaultGun();
                }

             
            }
        }
    }
    private void SetManager()
    {
        pInput.SetManager(this);
        pController.SetManager(this);
        pCamera.SetManager(this);
        pCorruption.SetManager(this);
        pHealth.SetManager(this);
        pUI.SetManager(this);
        pAnime.SetManager(this);
    }
    public void SetDeviceSettings()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput.devices.Count > 0)
        {
            var device = playerInput.devices[0];
            myDevice = device;
            if (device is Gamepad)
            {
                Debug.Log("I am using a gamepad");
                pCamera.SetGamepadSettings();
                bGun.structRotSpeed = 50;
                
            }
            else if (device is Keyboard || device is Mouse)
            {
                Debug.Log("I am using a keyboard");
                pCamera.SetMouseSettings();
                bGun.structRotSpeed = 25;
                
            }
        }
    }
    private void WeaponBehavior()
    {
        if (currentWeapon != null)
        {
            if (isFiring && !currentWeapon.IsReloading && currentWeapon is not ChargeGun)
            {
                currentWeapon.Fire();
                pUI.AmmoDisplay(currentWeapon);
            }
            else if (isCharging && (currentWeapon is ChargeGun gun))
            {
                // Charge weapons handle firing when the fire button is held
                gun.Charge();

                pUI.UpdateChargeGunSlider(gun.chargeAmount);
            }
        }
    }
    public void PickUpWeapon()
    {
        if (nearByWeapon == null)
            return;
        // Avoid getting Stuck in reload
        if(currentWeapon.IsReloading)
            currentWeapon.CancelReload();

        // deactivate the default weapon
        if (currentWeapon == defaultWeapon || currentWeapon == bGun)
        {
            currentWeapon.gameObject.SetActive(false);
        }
        else if (equippedWeapon != null)
        {
            Destroy(currentWeapon.gameObject); // Drop the current weapon
            equippedWeapon = null;
        }
        // Equip the new weapon
        currentWeapon = Instantiate(nearByWeapon, weaponHolder).GetComponent<Weapon>();
        // set the transforms of the new weapon
        currentWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        // set References
        currentWeapon.player = this;
        equippedWeapon = currentWeapon;
        // destroy pick up platform
        // disable pick up prompt
        pUI.DisablePrompt();
        // update UI to display new ammo capacities
        pUI.AmmoDisplay(currentWeapon);
        // update weapon icon
        pUI.SwitchWeaponIcon();

        if(currentWeapon is ChargeGun cGun)
        {
            pUI.ToggleChargeGunSlider(true);
        }

        // Animation switch depending on weapon type
        pAnime.SetWeaponHoldAnimation(currentWeapon.holdType);

        // if weapon is corrupted start corruption increase
        if (currentWeapon.isCorrupted)
        {
            holdingCorruption = true;
        }
        pUI.ToggleDefaultUI(true);
    }
    public void DropWeapon()
    {
        if (equippedWeapon == null || currentWeapon == bGun)
            return;
        Debug.Log("Dropping Weapon");
        if (currentWeapon != null)
        {
            if(currentWeapon.IsReloading)
                currentWeapon.CancelReload();

            if(currentWeapon is ChargeGun cGun)
            {
                pUI.ToggleChargeGunSlider(false);
            }
            Destroy(currentWeapon.gameObject); // Drop the current weapon
            if (Tutorial.Instance.currentScene == "Tutorial")
            {
                Tutorial.Instance.ProgressTutorial();
            }
        }

        currentWeapon = defaultWeapon;
        equippedWeapon = null;
        // reactivate the default weapon
        currentWeapon.gameObject.SetActive(true);
        holdingCorruption = false;
        pUI.AmmoDisplay(currentWeapon);
        pUI.SwitchWeaponIcon();
        pAnime.SetWeaponHoldAnimation(1);
    }
    public void EquipDefaultGun()
    {
        currentWeapon = defaultWeapon;
        currentWeapon.gameObject.SetActive(true);
        pAnime.SetWeaponHoldAnimation(currentWeapon.holdType);
        pUI.ToggleDefaultUI(true);

    }
    #region Toggle Switches
    public void ToggleBuildMode()
    {
        if(currentWeapon == null)
            return;

        if (!isBuilding)
        {// Enter Build mode
         // Avoid getting Stuck in reload
            if (currentWeapon.IsReloading)
                currentWeapon.CancelReload();
            currentWeapon.gameObject.SetActive(false);
            bGun.gameObject.SetActive(true);
            currentWeapon = bGun;
            pUI.buildUI.SetActive(true);
            
            pUI.EnableControls("<color=red>Build Mode</color> \n Mousewheel to change Structure \n Hold Right mouse to Preview \n F to enter Edit Mode");
            pUI.AmmoDisplay(currentWeapon);
            pUI.SwitchWeaponIcon();
            pAnime.SetWeaponHoldAnimation(1);
            isBuilding = true;
            if (Tutorial.Instance.currentScene == "Tutorial" && Tutorial.Instance.tutorialPrompt == 15)
            {
                Tutorial.Instance.ProgressTutorial();
            }
        }
        else
        {// Exit Build Mode
            if (bGun.isEditing)
            {
                bGun.ExitEditMode();
            }
            else
            {
                bGun.RemovePreview();
            }

            if (equippedWeapon != null)
                currentWeapon = equippedWeapon;
            else
                EquipDefaultGun();

            isBuilding = false;
            if (isFiring)
                pCamera.AimSight();
            else
                pCamera.DefaultSight();
            // if player is holding fire button when exiting, prevents auto shooting bug
            isFiring = false;
            bGun.gameObject.SetActive(false);
            currentWeapon.gameObject.SetActive(true);
            pUI.buildUI.SetActive(false);
            pUI.AmmoDisplay(currentWeapon);
            pUI.DisablePrompt();
            pUI.SwitchWeaponIcon();

            pAnime.SetWeaponHoldAnimation(currentWeapon.holdType);
        }
    }
    public void TogglePControl(bool toggle)
    {
        if (isBuilding)
            ToggleBuildMode();

        pController.gameObject.SetActive(toggle);
        if(toggle)
            pInput.EnableDefaultInputs();
        else
            pInput.DisableAllInputs();
    }
    public void TogglePCamera(bool toggle)
    {
        pCamera.gameObject.SetActive(toggle);
    }
    public void TogglePVisual(bool toggle)
    {
        // Fixes bug where player gun is stuck in reloading state...hopefully
        // since weapon is child of visual put in visual to activate whenever visual is disabled or enabled
        if (currentWeapon != null && currentWeapon.IsReloading)
            currentWeapon.CancelReload();
        pVisual.SetActive(toggle);
    }
    public void TogglePCorruption(bool toggle)
    {
        pCorruption.enabled  = toggle;
    }
    #endregion
    public void MovePlayerTo(Vector3 position)
    {
        pController.transform.position = position;
        
    }
    public void StartRespawn()
    {
        StartCoroutine(Respawn());
    }
    private IEnumerator Respawn()
    {
        TogglePControl(false);
        DropWeapon();
        if (isBuilding)
        {
            ToggleBuildMode();
        }

        yield return new WaitForSeconds(3);
        pAnime.ToggleRespawn(true);

        if (GameManager.Instance.trainHandler != null)
            MovePlayerTo(GameManager.Instance.trainHandler.playerSpawnPoint[GetPlayerIndex()].position);
        else
            MovePlayerTo(fallbackSpawnPoint.position);
        pCorruption.ResetCorruptionLevel();
        pHealth.ResetHealth();
        TogglePControl(true);
        if (pHealth.lives == 2)
        {
            pUI.life2.SetActive(false);
        }
        if (pHealth.lives == 1)
        {
            pUI.life1.SetActive(false);
        }
        yield return new WaitForSeconds (1);
        pAnime.ToggleRespawn(false);
    }
    public void GameOver()
    {
        Debug.Log("No Life No Game");
        SceneTransitioner.Instance.LoadLoseScene();
    }

    public int GetPlayerIndex()
    {
        return GetComponent<PlayerInput>().playerIndex;
    }
}
