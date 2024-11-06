using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputOrganizer : MonoBehaviour
{
    private PlayerManager pMan;
    // Input Maps
    private InputActionAsset inputAsset;
    private InputActionMap gameInputMap;
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
    private InputAction exitGame;
    private InputAction pauseGame;
    private InputAction toggleFullscreen;
    private InputAction flipCameraSide;
    // Shoot Actions
    private InputAction fireAction;
    private InputAction reloadAction;
    private InputAction dropAction;
    // Build Actions
    private InputAction buildModeAction;
    private InputAction changeStructAction;
    private InputAction placeStructAction;
    private InputAction enterEditMode;
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
        gameInputMap = inputAsset.FindActionMap("Game");
        playerInputMap = inputAsset.FindActionMap("Player");
        shootInputMap = inputAsset.FindActionMap("Shoot");
        buildInputMap = inputAsset.FindActionMap("Build");
        editInputMap = inputAsset.FindActionMap("Edit");
    }
    private void OnEnable()
    {
        // Assign actions from action map
        //Game Action Map
        exitGame = gameInputMap.FindAction("ExitGame");
        pauseGame = gameInputMap.FindAction("Pause");
        toggleFullscreen = gameInputMap.FindAction("ToggleFullscreen");
        // player action map
        moveAction = playerInputMap.FindAction("Move");
        lookAction = playerInputMap.FindAction("Look");
        jumpAction = playerInputMap.FindAction("Jump");
        sprintAction = playerInputMap.FindAction("Sprint");
        buildModeAction = playerInputMap.FindAction("Build");
        aimAction = playerInputMap.FindAction("Aim");
        interactAction = playerInputMap.FindAction("Interact");
        flipCameraSide = playerInputMap.FindAction("FlipCamera");
        // shoot action map
        reloadAction = shootInputMap.FindAction("Reload");
        dropAction = shootInputMap.FindAction("Drop");
        fireAction = shootInputMap.FindAction("Fire");
        // build Action Maps
        changeStructAction = buildInputMap.FindAction("Change");
        placeStructAction = buildInputMap.FindAction("Place");
        rotateStructAction = buildInputMap.FindAction("Rotate");
        enterEditMode = buildInputMap.FindAction("Edit");
        // Edit Action Maps
        moveStructAcion = editInputMap.FindAction("Move");
        destroyStructAction = editInputMap.FindAction("Destroy");
        upgradeStructAction = editInputMap.FindAction("Upgrade");
        exitEditAction = editInputMap.FindAction("Exit");
        // this enables the controls
        gameInputMap.Enable();
        playerInputMap.Enable();
        shootInputMap.Enable();
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
        pauseGame.performed += OnPause;
        exitGame.performed += OnExitGame;
        toggleFullscreen.performed += ToggleFullscreen;
        flipCameraSide.performed += OnFlipCamera;
        // shoot actions
        reloadAction.performed += OnReload;
        dropAction.performed += OnDropWeapon;
        // build actions
        changeStructAction.performed += OnCycleBuildStrcuture;
        placeStructAction.performed += OnPlaceStructure;
        enterEditMode.started += OnEnterEditMode;
        rotateStructAction.started += OnEditRotateStarted;
        rotateStructAction.canceled += OnEditRotateCancled;
        // Edit Actions
        moveStructAcion.started += OnEditStructureMoveStarted;
        moveStructAcion.canceled += OnEditMoveStructureCancled;
        exitEditAction.performed += OnExitEditMode;
        destroyStructAction.performed += OnEditDestroy;
        upgradeStructAction.started += OnEditUpgrade;

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
        pauseGame.performed -= OnPause;
        exitGame.performed -= OnExitGame;
        toggleFullscreen.performed -= ToggleFullscreen;
        flipCameraSide.performed -= OnFlipCamera;
        //shoot actions
        reloadAction.performed -= OnReload;
        dropAction.performed -= OnDropWeapon;
        // build Mode actions
        changeStructAction.performed -= OnCycleBuildStrcuture;
        placeStructAction.performed -= OnPlaceStructure;
        enterEditMode.started -= OnEnterEditMode;
        // edit Actions
        moveStructAcion.started -= OnEditStructureMoveStarted;
        moveStructAcion.canceled -= OnEditMoveStructureCancled;
        exitEditAction.performed -= OnExitEditMode;
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
    // interaction button will be the same button but do different things
    public void AssignInteraction(Interactables interaction)
    {
        interactAction.performed += interaction.Interact;
    }
    public void RemoveInteraction(Interactables interaction)
    {
        interactAction.performed -= interaction.Interact;
    }
    private void OnDropWeapon(InputAction.CallbackContext context)
    {
        pMan.DropWeapon();
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
    private void OnPause(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.WaveManager.paused)
        {
            if (pMan.isBuilding)
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

            playerInputMap.Disable();
            shootInputMap.Disable();

            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameManager.Instance.WaveManager.pauseMenu.OpenPauseMenu();
            GameManager.Instance.WaveManager.paused = true;
        }
        else
        {
            playerInputMap.Enable();
            shootInputMap.Enable();

            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.Instance.WaveManager.pauseMenu.ClosePauseMenu();
            GameManager.Instance.WaveManager.paused = false;
        }
   
    }
    private void OnExitGame(InputAction.CallbackContext context)
    {
        Debug.Log("Closing down Game");
        Application.Quit();
    }
    private void ToggleFullscreen(InputAction.CallbackContext context)
    {
        Debug.Log("Toggling fullscreen");

        if (Screen.fullScreen)
        {
            // turn off fullscreen
            Screen.SetResolution(960, 540, false);
        }
        else
        {
            Resolution defaultRes = Screen.currentResolution;
            // turn On fullscreen
            Screen.SetResolution(defaultRes.width, defaultRes.height, true);
        }

    }
    private void OnAimSightCall(InputAction.CallbackContext context)
    {
        pMan.pAnime.ToggleAimAnime(true);
        pMan.pCamera.AimSight();
    }
    private void OnDefaultSightCall(InputAction.CallbackContext context)
    {
        pMan.pAnime.ToggleAimAnime(false);
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
            pMan.currentWeapon.StartReload();
        }
    }
    private void OnFlipCamera(InputAction.CallbackContext context)
    {
        pMan.pCamera.FlipCameraSide();
    }
    // building stuff
    private void OnBuildMode(InputAction.CallbackContext context)
    {
        if (!pMan.isBuilding)
        {
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
    private void OnEnterEditMode(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {

            buildGun.EnterEditMode();

            rotateStructAction = editInputMap.FindAction("Rotate");
            rotateStructAction.started += OnEditRotateStarted;
            rotateStructAction.canceled += OnEditRotateCancled;

            pMan.pUI.EnablePrompt("<color=green>Edit Mode</color> \n RC to Move \n Hold X to Destroy \n Z to Upgrade \n F to return");
            buildInputMap.Disable();
            editInputMap.Enable();
        }
    }
    private void OnExitEditMode(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.ExitEditMode();
            
            rotateStructAction = buildInputMap.FindAction("Rotate");
            rotateStructAction.started -= OnEditRotateStarted;
            rotateStructAction.canceled -= OnEditRotateCancled;

            pMan.pUI.EnablePrompt("<color=red>Build Mode</color> \nUse Q/E to change Structure" + "\n F to Select Structure" + "\n Hold Right mouse to Preview");
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
        }
    }
    private void OnEditDestroy(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.DestroyStructure();

            editInputMap.Disable();
            buildInputMap.Enable();
        }
    }
    private void OnEditRotateStarted(InputAction.CallbackContext context)
    {
        pMan.isRotating = true;
    }
    private void OnEditRotateCancled(InputAction.CallbackContext context)
    {
        pMan.isRotating = false;
    }
    private void OnEditUpgrade(InputAction.CallbackContext context)
    {
        // put upgrade code here
        pMan.bGun.UpgradeStructure();
    }

    public string GetInteractionKey()
    {
        string key = interactAction.GetBindingDisplayString();

        return key;
    }

    #endregion
}
