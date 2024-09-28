using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

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
    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction jumpAction;
    public InputAction aimAction;
    public InputAction fireAction;
    public InputAction interactAction;
    // Shoot Actions
    public InputAction reloadAction;
    public InputAction pickUpAction;
    public InputAction dropAction;
    // Build Actions
    private InputAction buildModeAction;
    private InputAction changeBuildAction;
    private InputAction placeBuildAction;
    private InputAction selectStructAction;
    // Edit Actions
    private InputAction moveStructAcion;
    public InputAction rotateStructAction;
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
        buildModeAction = playerInputMap.FindAction("Build");
        aimAction = playerInputMap.FindAction("Aim");
        interactAction = playerInputMap.FindAction("Interact");
        // shoot action map
        reloadAction = shootInputMap.FindAction("Reload");
        pickUpAction = shootInputMap.FindAction("PickUp");
        dropAction = shootInputMap.FindAction("Drop");
        fireAction = shootInputMap.FindAction("Fire");
        // build Action Maps
        changeBuildAction = buildInputMap.FindAction("Change");
        placeBuildAction = buildInputMap.FindAction("Place");
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
        dropAction.performed += OnDropWeapon;
        // build actions
        changeBuildAction.started += OnCycleBuildStrcuture;
        placeBuildAction.started += OnPlaceStructure;
        selectStructAction.started += OnSelectStructure;
        rotateStructAction.started += OnEditRotateStarted;
        rotateStructAction.canceled += OnEditRotateCancled;
        // Edit Actions
        moveStructAcion.started += OnEditStructureMoveStarted;
        moveStructAcion.canceled += OnEditMoveStructureCancled;
        exitEditAction.started += OnExitEditStructure;
        destroyStructAction.performed += OnEditDestroy;
        upgradeStructAction.started += OnEditUpgrade;
    }
    public void AssignInteraction()
    {
        interactAction.started += OnPushButton;
    }
    public void RemoveInteraction()
    {
        interactAction.started -= OnPushButton;
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
        dropAction.performed -= OnDropWeapon;
        // build Mode actions
        changeBuildAction.started -= OnCycleBuildStrcuture;
        placeBuildAction.started -= OnPlaceStructure;
        selectStructAction.started -= OnSelectStructure;
        // edit Actions
        moveStructAcion.started -= OnEditStructureMoveStarted;
        moveStructAcion.canceled -= OnEditMoveStructureCancled;
        exitEditAction.started -= OnExitEditStructure;
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

            //Change build Actions
            fireAction = buildInputMap.FindAction("Preview");
            
            // Assign build mode actions
            fireAction.started += OnFireStarted;
            fireAction.canceled += OnFireCanceled;
            
            pMan.SetBuildMode();
            shootInputMap.Disable();
            editInputMap.Disable();
            buildInputMap.Enable();
        }
        else
        {
            Debug.Log(message: "Disable Build Mode");
            fireAction = shootInputMap.FindAction("Fire");
            // Assign build mode actions
            fireAction.started += OnFireStarted;
            fireAction.canceled += OnFireCanceled;

            pMan.bGun.DestroySelectedObject();
            pMan.SetBuildMode();
            buildInputMap.Disable();
            editInputMap.Disable();
            shootInputMap.Enable();
        }

    }
    private void OnCycleBuildStrcuture(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.CycleSelectedStructure(changeBuildAction.ReadValue<float>()); // Cycle through build able objects
            pMan.pUI.AmmoDisplay(pMan.currentWeapon);
        }
    }
    private void OnPlaceStructure(InputAction.CallbackContext context)
    {
        pMan.bGun.PlaceStructure();
    }
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
    }
    // Weapon Pick up
    private void OnPickUpWeapon(InputAction.CallbackContext context)
    {
        pMan.PickUpWeapon();
    }
    private void OnDropWeapon(InputAction.CallbackContext context)
    {
        pMan.DropWeapon();
    }
}
