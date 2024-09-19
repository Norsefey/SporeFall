using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement pController;
    public TPSCamera pCamera;
    public PlayerUI pUI;
    public GameObject pVisual;
    
    // Input Maps
    private InputActionAsset inputAsset;
    private InputActionMap player;
    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction jumpAction;
    public InputAction aimAction;
    public InputAction fireAction;
    public InputAction reloadAction;

    [Header("Weapons")]
    // Weapons and Shooting
    public Transform weaponHolder; // Where the weapon is equipped
    public Weapon currentWeapon;
    
    private bool isFiring = false;
    private bool isCharging = false;
    private void Awake()
    {
        // get and assign input Action Map
        inputAsset = this.GetComponent<PlayerInput>().actions;
        player = inputAsset.FindActionMap("Player");

        // since we will have multiple players, this manager cannot be a public instance, so we assign it locally
        pController.SetManager(this);
        pCamera.SetManager(this);

    }
    private void Start()
    {
        // in order to spawn player at a spawn point, disable movement controls
        pController.gameObject.SetActive(true);
        pUI.AmmoDisplay(currentWeapon);

    }
    private void Update()
    {
        if (currentWeapon != null)
        {
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
    private void OnEnable()
    {
        // Find and assign actions
        moveAction = player.FindAction("Move");
        lookAction = player.FindAction("Look");
        jumpAction = player.FindAction("Jump");
        aimAction = player.FindAction("Aim");
        fireAction = player.FindAction("Fire");
        reloadAction = player.FindAction("Reload");

        // Assign Calls to each action
        jumpAction.started += pController.JumpCall;
        aimAction.started += pCamera.AimSightCall;
        aimAction.canceled += pCamera.DefaultSightCall;
        fireAction.started += OnFireStarted;
        fireAction.canceled += OnFireCanceled;
        reloadAction.performed += OnReload;
      
        // this enables the controls
        player.Enable();
    }
    private void OnDisable()
    {
        // remove calls
        jumpAction.started -= pController.JumpCall;
        aimAction.started -= pCamera.AimSightCall;
        aimAction.canceled -= pCamera.DefaultSightCall;
        fireAction.started -= OnFireStarted;
        fireAction.canceled -= OnFireCanceled;
        reloadAction.performed -= OnReload;

        // disable Input map
        player.Disable();
    }
    // Called when the Fire action is triggered
    private void OnFireStarted(InputAction.CallbackContext context)
    {
        if (currentWeapon is ChargeGun)
        {
            isCharging = true;
        }
        else
        {
            isFiring = true;
        }
    }
    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        if (currentWeapon is ChargeGun gun)
        {
            isCharging = false; // Fire the charged shot when fire button is released
            gun.Release();
            pUI.AmmoDisplay(currentWeapon);

        }else if (currentWeapon is BurstGun burstGun)
        {
            burstGun.OnFireReleased(); // Call the release method on the burst weapon
        }
        isFiring = false;
    }
    private void OnReload(InputAction.CallbackContext context)
    {
        Debug.Log("Reloading");

        if (currentWeapon != null)
        {
            currentWeapon.Reload();
        }        
    }
    // Called when the Reload action is triggered
    public void PickUpWeapon(Weapon newWeapon)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject); // Drop the current weapon
        }

        // Equip the new weapon
        currentWeapon = Instantiate(newWeapon, weaponHolder);
        currentWeapon.player = this;
        // update UI to display new ammo capacities
        pUI.AmmoDisplay(currentWeapon);

        Debug.Log("Picked up: " + currentWeapon.weaponName);
    }
}
