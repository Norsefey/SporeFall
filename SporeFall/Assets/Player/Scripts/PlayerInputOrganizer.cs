using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerInputOrganizer : MonoBehaviour
{
    private PlayerManager pMan;
    // Input Maps
    private InputActionAsset inputAsset;
    private InputActionMap playerInputMap;
    public InputActionMap shootInputMap;
    public InputActionMap buildInputMap;
    [Header("Input Actions")]// sorted into their respective action maps
    // Player Actions
    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction jumpAction;
    public InputAction aimAction;
    public InputAction fireAction;
    // Shoot Actions
    public InputAction reloadAction;
    public InputAction pickUpAction;
    // Build Actions
    private InputAction buildModeAction;
    private InputAction changeBuildAction;
    private InputAction placeBuildAction;



    private void Awake()
    {
        // get and assign input Action Map
        inputAsset = this.GetComponent<PlayerInput>().actions;
        playerInputMap = inputAsset.FindActionMap("Player");
        shootInputMap = inputAsset.FindActionMap("Shoot");
        buildInputMap = inputAsset.FindActionMap("Build");
    }
    private void OnEnable()
    {
        // Assign actions from action map
        // player action map
        moveAction = playerInputMap.FindAction("Move");
        lookAction = playerInputMap.FindAction("Look");
        jumpAction = playerInputMap.FindAction("Jump");
        buildModeAction = playerInputMap.FindAction("Build");
        aimAction = playerInputMap.FindAction("Aim");
        fireAction = playerInputMap.FindAction("Fire");
        // shoot action map
        reloadAction = shootInputMap.FindAction("Reload");
        pickUpAction = shootInputMap.FindAction("PickUp");
        //build Actions
        changeBuildAction = buildInputMap.FindAction("Change");
        placeBuildAction = buildInputMap.FindAction("Place");

        // this enables the controls
        playerInputMap.Enable();
        shootInputMap.Enable();
        buildInputMap.Disable();
    }
    private void OnDisable()
    {
        // remove calls
        jumpAction.started -= pMan.pController.JumpCall;
        aimAction.started -= pMan.pCamera.AimSightCall;
        aimAction.canceled -= pMan.pCamera.DefaultSightCall;
        fireAction.started -= OnFireStarted;
        fireAction.canceled -= OnFireCanceled;
        buildModeAction.started -= OnBuildMode;
        //shoot actions
        reloadAction.performed -= OnReload;
        pickUpAction.performed -= OnPickUpWeapon;
        // build Mode actions
        changeBuildAction.started -= OnCycleBuildObject;
        placeBuildAction.started -= OnPlaceObject;

        // disable Input map
        playerInputMap.Disable();
        shootInputMap.Disable();
    }
    public void AssignAllActions()
    {
        // Assign Calls to each action
        // basic player actions
        jumpAction.started += pMan.pController.JumpCall;
        aimAction.started += pMan.pCamera.AimSightCall;
        aimAction.canceled += pMan.pCamera.DefaultSightCall;
        fireAction.started += OnFireStarted;
        fireAction.canceled += OnFireCanceled;
        buildModeAction.started += OnBuildMode;
        // shoot actions
        reloadAction.performed += OnReload;
        pickUpAction.performed += OnPickUpWeapon;
        // build mode actions
        changeBuildAction.started += OnCycleBuildObject;
        placeBuildAction.started += OnPlaceObject;
    }
    public void SetManager(PlayerManager manger)
    {
        pMan = manger;
    }
    // Called when the Fire action is triggered
    private void OnFireStarted(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is ChargeGun)
        {
            pMan.isCharging = true;
        }
        else
        {
            pMan.isFiring = true;
        }
    }
    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        switch (pMan.currentWeapon)
        {
            case ChargeGun:
                pMan.isCharging = false; // Fire the charged shot when fire button is released
                ((ChargeGun)pMan.currentWeapon).Release();
                pMan.pUI.AmmoDisplay(pMan.currentWeapon);
                break;
            case BurstGun:
                ((BurstGun)pMan.currentWeapon).OnFireReleased(); // Call the release method on the burst weapon
                break;
            case BuildGun:
                ((BuildGun)pMan.currentWeapon).OnFireReleased(); // Call the release method on the burst weapon
                break;
            default:
                break;
        }
        pMan.isFiring = false;
    }
    private void OnReload(InputAction.CallbackContext context)
    {
        Debug.Log("Reloading");

        if (pMan.currentWeapon != null)
        {
            pMan.currentWeapon.Reload();
        }
    }
    // building stuff
    private void OnBuildMode(InputAction.CallbackContext context)
    {
        pMan.SetBuildMode();
    }
    private void OnCycleBuildObject(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.CycleBuildObject(); // Cycle through build able objects
            pMan.pUI.AmmoDisplay(pMan.currentWeapon);
        }
    }
    private void OnPlaceObject(InputAction.CallbackContext context)
    {
        pMan.bGun.PlaceObject();
    }
    private void OnSelectObject(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.SelectObject(); // Select a placed object for moving or destroying
        }
    }
    private void OnMoveObject(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.MoveSelectedObject(); // Move the selected object to a new location
        }
    }
    private void OnDestroyObject(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.DestroySelectedObject(); // Destroy the selected object
        }
    }
    private void OnPickUpWeapon(InputAction.CallbackContext context)
    {
        pMan.PickUpWeapon();
    }
}
