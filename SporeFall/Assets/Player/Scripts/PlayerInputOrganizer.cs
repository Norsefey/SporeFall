using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputOrganizer : MonoBehaviour
{
    private PlayerManager pMan;
    // Input Maps
    private InputActionAsset inputAsset;
    private InputActionMap playerInputMap;
    private InputActionMap shootInputMap;
    private InputActionMap buildInputMap;
    private InputActionMap editInputMap;
    [Header("Input Actions")]// sorted into their respective action maps
    // Player Actions
    public  InputAction moveAction;
    public  InputAction lookAction;
    private InputAction jumpAction;
    private InputAction aimAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    // Shoot Actions
    private InputAction fireAction;
    private InputAction reloadAction;
    private InputAction dropAction;
    // Build Actions
    private InputAction buildModeAction;
    private InputAction changeStructAction;
    private InputAction placeStructAction;
    private InputAction selectStructAction;
    // Edit Actions
    public  InputAction rotateStructAction;
    private InputAction moveStructAcion;
    private InputAction destroyStructAction;
    private InputAction upgradeStructAction;
    private InputAction exitEditAction;
    private void Awake()
    {
        // get and assign input Action Map
        inputAsset = this.GetComponent<PlayerInput>().actions;
        playerInputMap = inputAsset.FindActionMap("Player");
        shootInputMap = inputAsset.FindActionMap("Shoot");
        buildInputMap = inputAsset.FindActionMap("Build");
        editInputMap = inputAsset.FindActionMap("Edit");
    }
    private void OnEnable()
    {
        // Assign actions from action map
        // player action map
        moveAction = playerInputMap.FindAction("Move");
        lookAction = playerInputMap.FindAction("Look");
        jumpAction = playerInputMap.FindAction("Jump");
        sprintAction = playerInputMap.FindAction("Sprint");
        buildModeAction = playerInputMap.FindAction("Build");
        aimAction = playerInputMap.FindAction("Aim");
        interactAction = playerInputMap.FindAction("Interact");
        // shoot action map
        reloadAction = shootInputMap.FindAction("Reload");
        dropAction = shootInputMap.FindAction("Drop");
        fireAction = shootInputMap.FindAction("Fire");
        // build Action Maps
        changeStructAction = buildInputMap.FindAction("Change");
        placeStructAction = buildInputMap.FindAction("Place");
        selectStructAction = buildInputMap.FindAction("Select");
        rotateStructAction = buildInputMap.FindAction("Rotate");
        // Edit Action Maps
        moveStructAcion = editInputMap.FindAction("Move");
        destroyStructAction = editInputMap.FindAction("Destroy");
        upgradeStructAction = editInputMap.FindAction("Upgrade");
        exitEditAction = editInputMap.FindAction("Exit");
        // this enables the controls
        playerInputMap.Enable();
        shootInputMap.Enable();
        buildInputMap.Disable();
        editInputMap.Disable();
    }
    private void OnDisable()
    {
        // remove calls
        jumpAction.started -= OnJumpCall;
        sprintAction.started -= OnSprintStarted;
        sprintAction.canceled -= OnSprintCanceled;
        aimAction.started -= OnAimSightCall;
        aimAction.canceled -= OnDefaultSightCall;
        fireAction.started -= OnFireStarted;
        fireAction.canceled -= OnFireCanceled;
        buildModeAction.started -= OnBuildMode;
        //shoot actions
        reloadAction.performed -= OnReload;
        dropAction.performed -= OnDropWeapon;
        // build Mode actions
        changeStructAction.performed -= OnCycleBuildStrcuture;
        placeStructAction.performed -= OnPlaceStructure;
        selectStructAction.started -= OnSelectStructure;
        // edit Actions
        moveStructAcion.started -= OnEditStructureMoveStarted;
        moveStructAcion.canceled -= OnEditMoveStructureCancled;
        exitEditAction.performed -= OnExitEditStructure;
        destroyStructAction.performed -= OnEditDestroy;
        rotateStructAction.started -= OnEditRotateStarted;
        rotateStructAction.canceled -= OnEditRotateCancled;
        upgradeStructAction.started -= OnEditUpgrade;
        // disable Input map
        playerInputMap.Disable();
        shootInputMap.Disable();
        buildInputMap.Disable();
        editInputMap.Disable();
    }
    public void AssignAllActions()
    {
        // Assign Calls to each action
        // basic player actions
        jumpAction.started += OnJumpCall;
        sprintAction.started += OnSprintStarted;
        sprintAction.canceled += OnSprintCanceled;
        aimAction.started += OnAimSightCall;
        aimAction.canceled += OnDefaultSightCall;
        fireAction.started += OnFireStarted;
        fireAction.canceled += OnFireCanceled;
        buildModeAction.started += OnBuildMode;
        // shoot actions
        reloadAction.performed += OnReload;
        dropAction.performed += OnDropWeapon;
        // build actions
        changeStructAction.performed += OnCycleBuildStrcuture;
        placeStructAction.performed += OnPlaceStructure;
        selectStructAction.started += OnSelectStructure;
        rotateStructAction.started += OnEditRotateStarted;
        rotateStructAction.canceled += OnEditRotateCancled;
        // Edit Actions
        moveStructAcion.started += OnEditStructureMoveStarted;
        moveStructAcion.canceled += OnEditMoveStructureCancled;
        exitEditAction.performed += OnExitEditStructure;
        destroyStructAction.performed += OnEditDestroy;
        upgradeStructAction.started += OnEditUpgrade;
    }
    
    // interaction button will be the same button but do different things
    public void AssignWeaponPickUp()
    {
        interactAction.performed += OnPickUpWeapon;
    }
    public void RemoveWeaponPickUp()
    {
        interactAction.performed -= OnPickUpWeapon;
    }
    public void AssignButtonPush()
    {
        interactAction.performed += OnPushButton;
    }
    public void RemoveButtonPush()
    {
        interactAction.performed -= OnPushButton;
    }
    // Disabling All Inputs
    public void DisableAllInputs()
    {
        playerInputMap.Disable();
        shootInputMap.Disable();
        buildInputMap.Disable();
        editInputMap.Disable();
    }
    public void EnableDefaultInputs()
    {
        playerInputMap.Enable();
        shootInputMap.Enable();
        buildInputMap.Disable();
        editInputMap.Disable();
    }
    // Setting the player manager
    public void SetManager(PlayerManager manger)
    {
        pMan = manger;
    }
    
    #region input Calls
    private void OnAimSightCall(InputAction.CallbackContext context)
    {
        pMan.pCamera.AimSight();
    }
    private void OnDefaultSightCall(InputAction.CallbackContext context)
    {
        pMan.pCamera.DefaultSight();
    }
    private void OnJumpCall(InputAction.CallbackContext context)
    {
        if (pMan != null)
        {
            pMan.pController.JumpCall();
        }
    }
    private void OnSprintStarted(InputAction.CallbackContext context)
    {
        pMan.pController.SetSprintSpeed(true);
    }
    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        pMan.pController.SetSprintSpeed(false);
    }
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
                ((BuildGun)pMan.currentWeapon).OnFireReleased(); // Call the release method on the Build weapon
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
    // Interaction
    private void OnPushButton(InputAction.CallbackContext context)
    {
        pMan.OnButtonPush();
    }
    // building stuff
    private void OnBuildMode(InputAction.CallbackContext context)
    {
        if (!pMan.isBuilding)
        {
            Debug.Log(message: "Enabling Build Mode");
            // if player is holding fire, prevent press from carrying over
            fireAction.Disable();
            //Change build Actions
            fireAction = buildInputMap.FindAction("Preview");
            
            // Assign build mode actions
            fireAction.started += OnFireStarted;
            fireAction.canceled += OnFireCanceled;
            
            pMan.ToggleBuildMode();
            shootInputMap.Disable();
            editInputMap.Disable();
            buildInputMap.Enable();
            // After Build map is enabled re enable fire action
            fireAction.Enable();
        }
        else
        {
            // if player is holding fire, prevent press from carrying over
            // Fixes auto shoot bug
            fireAction.Disable();

            fireAction = shootInputMap.FindAction("Fire");
            // Assign build mode actions
            fireAction.started += OnFireStarted;
            fireAction.canceled += OnFireCanceled;

            pMan.ToggleBuildMode();

            editInputMap.Disable();
            buildInputMap.Disable();
            shootInputMap.Enable();
            // After Shoot map is enabled re enable fire action
            fireAction.Enable();
        }

    }
    private void OnCycleBuildStrcuture(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.CycleSelectedStructure(changeStructAction.ReadValue<float>()); // Cycle through build able objects
            pMan.pUI.AmmoDisplay(pMan.currentWeapon);
        }
    }
    private void OnPlaceStructure(InputAction.CallbackContext context)
    {
        pMan.bGun.PlaceStructure();
    }
    // Editing a Structure
    private void OnSelectStructure(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            if (buildGun.SelectStructure()) // Select a placed object for moving or destroying
            {
                buildGun.isEditing = true;

                rotateStructAction = editInputMap.FindAction("Rotate");
                rotateStructAction.started += OnEditRotateStarted;
                rotateStructAction.canceled += OnEditRotateCancled;
                
                pMan.pUI.EnablePrompt("RC to Move \n Hold X to Destroy \n Z to Upgrade \n F to return");
                buildInputMap.Disable();
                editInputMap.Enable();
            }
        }
    }
    private void OnExitEditStructure(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.DeSelectStructure();
            
            rotateStructAction = buildInputMap.FindAction("Rotate");
            rotateStructAction.started += OnEditRotateStarted;
            rotateStructAction.canceled += OnEditRotateCancled;

            pMan.pUI.EnablePrompt("Use Q/E to change Structure" + "\n F to Select Structure" + "\n Hold Right mouse to Preview");
            editInputMap.Disable();
            buildInputMap.Enable();
        }
    }
    private void OnEditStructureMoveStarted(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            pMan.isFiring = true;
            buildGun.isEditing = true;
            buildGun.Fire();
        }
    }
    private void OnEditMoveStructureCancled(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.OnFireReleased();
            pMan.isFiring = false;
            buildGun.isEditing = false;
        }
    }
    private void OnEditDestroy(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.DestroySelectedObject();

            editInputMap.Disable();
            buildInputMap.Enable();
        }
    }
    private void OnEditRotateStarted(InputAction.CallbackContext context)
    {
        Debug.Log("CanRotate");
        pMan.isRotating = true;
    }
    private void OnEditRotateCancled(InputAction.CallbackContext context)
    {
        Debug.Log("No Rotate");

        pMan.isRotating = false;
    }
    private void OnEditUpgrade(InputAction.CallbackContext context)
    {
        // put upgrade code here
        pMan.bGun.UpgradeStructure();
    }
    // Weapon Pick up
    private void OnPickUpWeapon(InputAction.CallbackContext context)
    {
        Debug.Log("Picking up weapon");
        pMan.PickUpWeapon();
    }
    private void OnDropWeapon(InputAction.CallbackContext context)
    {
        pMan.DropWeapon();
    }
    #endregion
}
