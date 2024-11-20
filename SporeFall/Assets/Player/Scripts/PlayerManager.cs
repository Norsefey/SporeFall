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
    public TrainHandler train;
    public Interactables interactable;
    [Header("Weapons")]
    // Weapons and Shooting
    public Transform weaponHolder; // Where the weapon is equipped
    public Weapon currentWeapon;
    public Weapon defaultWeapon;
    public Weapon equippedWeapon;
    // pickables
    public GameObject nearByWeapon;

    public bool isFiring = false;
    public bool isCharging = false;
    public bool isBuilding = false;
    public bool isRotating = false;
    [Header("Currency")]
    // Player Stats
    private float mycelia = 125;
    public float Mycelia { get { return mycelia; } }
    [Header("Respawn")]
    [SerializeField] private float respawnTime;
    [SerializeField] private Transform fallbackSpawnPoint;

    public bool holdingCorruption = false;
    private int corruptionPickupCount = 0;
    public InputDevice myDevice;
    private void Awake()
    {
        pInput = GetComponent<PlayerInputOrganizer>();
        // since we will have multiple players, this manager cannot be a public instance, so we assign it locally
        SetManager();
        SetDeviceSettings();
        Tutorial.Instance.playerActive = true;
        Debug.Log("Player is awake");
    }
    private void Start()
    {
        GameManager.Instance.TrainHandler.AddPlayer(this);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pUI.AmmoDisplay(currentWeapon);
        pInput.AssignAllActions();

        

    }
    private void Update()
    {
        WeaponBehavior();
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
            if (currentWeapon is BuildGun bGun)
            {
                pUI.DisplayMycelia(mycelia);
                if (bGun.isEditing)
                {
                    if (bGun.SelectStructure())
                        bGun.RotateStructure();
                }
            }

            if (isFiring && !currentWeapon.IsReloading && currentWeapon is not ChargeGun)
            {
                currentWeapon.Fire();
                pUI.AmmoDisplay(currentWeapon);
            }
            else if (isCharging && (currentWeapon is ChargeGun gun))
            {
                // Charge weapons handle firing when the fire button is held
                gun.Charge();
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

        // Animation switch depending on weapon type
        if(currentWeapon.isTwoHanded)
            pAnime.ToggleTwoHanded(true);
        else
            pAnime.ToggleTwoHanded(false);

        // if weapon is corrupted start corruption increase
        if (currentWeapon.isCorrupted)
            holdingCorruption = true;
            corruptionPickupCount++;
            if (corruptionPickupCount == 1)
            {
                 Tutorial.Instance.firstCorruptionPickup = true;
            }
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
            Destroy(currentWeapon.gameObject); // Drop the current weapon
        }

        currentWeapon = defaultWeapon;
        equippedWeapon = null;
        // reactivate the default weapon
        currentWeapon.gameObject.SetActive(true);
        holdingCorruption = false;
        pUI.AmmoDisplay(currentWeapon);
        pUI.SwitchWeaponIcon();
        pAnime.ToggleTwoHanded(false);
    }
    #region Toggle Switches
    public void ToggleBuildMode()
    {
        if (!isBuilding)
        {// Enter Build mode
         // Avoid getting Stuck in reload
            if (currentWeapon.IsReloading)
                currentWeapon.CancelReload();
            currentWeapon.gameObject.SetActive(false);
            bGun.gameObject.SetActive(true);
            currentWeapon = bGun;
            pUI.buildUI.SetActive(true);
            
            pUI.EnableControls("<color=red>Build Mode</color> \n Q/E to change Structure \n Hold Right mouse to Preview" + "\n F to Select Placed Structure");
            pUI.AmmoDisplay(currentWeapon);
            pUI.SwitchWeaponIcon();
            pAnime.ToggleTwoHanded(false);
            isBuilding = true;
        }
        else
        {// Exit Build Mode
            if (bGun.isEditing)
            {
                bGun.ExitEditMode();
            }
            else
            {
                bGun.DestroyPreview();
            }

            if (equippedWeapon != null)
                currentWeapon = equippedWeapon;
            else
                currentWeapon = defaultWeapon;
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

            if (currentWeapon.isTwoHanded)
                pAnime.ToggleTwoHanded(true);
            else
                pAnime.ToggleTwoHanded(false);
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
        if (currentWeapon.IsReloading)
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
        yield return new WaitForSeconds(2);
        if (train != null)
            MovePlayerTo(train.playerSpawnPoint[GetPlayerIndex()].position);
        else
            MovePlayerTo(fallbackSpawnPoint.position);
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
    }
    public void GameOver()
    {
        Debug.Log("No Life No Game");
        SceneTransitioner.Instance.LoadLoseScene();
    }
    public void IncreaseMycelia(float amount)
    {
        mycelia += amount;
        pUI.DisplayMycelia(mycelia);
    }
    public void DecreaseMycelia(float amount)
    {
        mycelia -= amount;
        pUI.DisplayMycelia(mycelia);
    }
    public int GetPlayerIndex()
    {
        return GetComponent<PlayerInput>().playerIndex;
    }
}
