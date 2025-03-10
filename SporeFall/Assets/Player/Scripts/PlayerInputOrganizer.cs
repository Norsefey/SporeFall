using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
public class PlayerInputOrganizer : MonoBehaviour
{
    private PlayerManager pMan;
    // Input Maps
    private InputActionAsset inputAsset;
    private InputActionMap gameInputMap;
    private InputActionMap playerInputMap;
    private InputActionMap shootInputMap;
    private InputActionMap buildInputMap;
    private InputActionMap placementInputMap;
    private InputActionMap editInputMap;
    [Header("Input Actions")]// sorted into their respective action maps
    //Game Actions
    private InputAction exitGame;
    private InputAction pauseGame;
    private InputAction leaveGame;
    private InputAction toggleFullscreen;
    private InputAction skipCutscene;
    // Player Actions
    public InputAction moveAction;
    public  InputAction lookAction;
    private InputAction jumpAction;
    private InputAction aimAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction flipCameraSide;
    private InputAction buildModeAction;
    private InputAction tutorialAction;
    // Shoot Actions
    private InputAction fireAction;
    private InputAction reloadAction;
    private InputAction dropAction;
    // Build Actions
    private InputAction showStructureRadius;
    private InputAction editModeAction;
    private InputAction rotateStructAction;
    // Placement Actions
    private InputAction changeStructAction;
    private InputAction placeStructAction;
    // Edit Actions
    private InputAction moveStructAcion;
    private InputAction destroyStructAction;
    // have to store this, since Hold needs to be check first, and after that, release value is just zero
    private float rotationDirection = 0;

    private void Awake()
    {
        // get and assign input Action Map
        inputAsset = this.GetComponent<PlayerInput>().actions;
        gameInputMap = inputAsset.FindActionMap("Game");
        playerInputMap = inputAsset.FindActionMap("Player");
        shootInputMap = inputAsset.FindActionMap("Shoot");
        buildInputMap = inputAsset.FindActionMap("Build");
        placementInputMap = inputAsset.FindActionMap("BuildPlacement");
        editInputMap = inputAsset.FindActionMap("BuildEdit");
    }
    private void OnEnable()
    {
        // Assign actions from action map
        //Game Action Map
        exitGame = gameInputMap.FindAction("ExitGame");
        pauseGame = gameInputMap.FindAction("Pause");
        toggleFullscreen = gameInputMap.FindAction("ToggleFullscreen");
        skipCutscene = gameInputMap.FindAction("SkipCutscene");
        leaveGame = gameInputMap.FindAction("Leave");
        // player action map
        moveAction = playerInputMap.FindAction("Move");
        lookAction = playerInputMap.FindAction("Look");
        sprintAction = playerInputMap.FindAction("Sprint");
        buildModeAction = playerInputMap.FindAction("Build");
        aimAction = playerInputMap.FindAction("Aim");
        interactAction = playerInputMap.FindAction("Interact");
        flipCameraSide = playerInputMap.FindAction("FlipCamera");
        tutorialAction = playerInputMap.FindAction("ProgressTutorial");
        // shoot action map
        jumpAction = shootInputMap.FindAction("Jump");
        reloadAction = shootInputMap.FindAction("Reload");
        dropAction = shootInputMap.FindAction("Drop");
        fireAction = shootInputMap.FindAction("Fire");
        // build Action Maps
        showStructureRadius = buildInputMap.FindAction("ToggleRadius");
        rotateStructAction = buildInputMap.FindAction("Rotate");
        editModeAction = buildInputMap.FindAction("EditMode");
        // Placement Action map
        changeStructAction = placementInputMap.FindAction("Change");
        placeStructAction = placementInputMap.FindAction("Place");
        // Edit Action Maps
        moveStructAcion = editInputMap.FindAction("Move");
        destroyStructAction = editInputMap.FindAction("Destroy");

        // player starts in default mode
        gameInputMap.Enable();
        playerInputMap.Enable();
        shootInputMap.Enable();

        buildInputMap.Disable();
        placementInputMap.Disable();
        editInputMap.Disable();
    }
    public void AssignAllActions()
    {
        // Assign Calls to each action
        // basic player actions
        skipCutscene.performed += OnSkipCutscene;
        leaveGame.performed += OnLeaveGame;
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
        tutorialAction.performed += OnProgressTutorial;
        showStructureRadius.performed += OnToggleShowRadius;
        // shoot actions
        reloadAction.performed += OnReload;
        dropAction.performed += OnDropWeapon;
        // build actions
        editModeAction.started += OnToggleEditMode;
        rotateStructAction.started += OnRotateStarted;
        rotateStructAction.performed += OnRotate;
        // Placement Actions
        changeStructAction.performed += OnCycleBuildStrcuture;
        placeStructAction.performed += OnPlaceStructure;
        // Edit Actions
        moveStructAcion.started += OnEditStructureMoveStarted;
        moveStructAcion.canceled += OnEditMoveStructureCancled;
        destroyStructAction.performed += OnEditDestroy;
    }
    private void OnDisable()
    {
        // remove calls
        leaveGame.performed -= OnLeaveGame;
        skipCutscene.performed -= OnSkipCutscene;
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
        tutorialAction.performed -= OnProgressTutorial;
        showStructureRadius.performed -= OnToggleShowRadius;
        //shoot actions
        reloadAction.performed -= OnReload;
        dropAction.performed -= OnDropWeapon;
        // build Mode actions
        changeStructAction.performed -= OnCycleBuildStrcuture;
        placeStructAction.performed -= OnPlaceStructure;
        editModeAction.started -= OnToggleEditMode;
        rotateStructAction.started -= OnRotateStarted;
        rotateStructAction.performed -= OnRotate;

        // edit Actions
        moveStructAcion.started -= OnEditStructureMoveStarted;
        moveStructAcion.canceled -= OnEditMoveStructureCancled;
        destroyStructAction.performed -= OnEditDestroy;
        // disable Input map
        gameInputMap.Disable();
        playerInputMap.Disable();
        shootInputMap.Disable();
        buildInputMap.Disable();
        placementInputMap.Disable();
        editInputMap.Disable();
    }
    // interaction button will be the same button but do different things
    public void AssignInteraction(Interactables interaction)
    {
        if(pMan.interactable != null)
        {
            interactAction.performed -= pMan.interactable.Interact;
        }

        interactAction.performed += interaction.Interact;
        pMan.interactable = interaction;
    }
    public void RemoveInteraction(Interactables interaction)
    {
        if(pMan.interactable == null)
            return;

        interactAction.performed -= interaction.Interact;
        pMan.interactable = null;
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
        placementInputMap.Disable();
        editInputMap.Disable();
    }
    public void EnableDefaultInputs()
    {
        playerInputMap.Enable();
        shootInputMap.Enable();
        buildInputMap.Disable();
        placementInputMap.Disable();
        editInputMap.Disable();
    }
    // Setting the player manager to use as reference
    public void SetManager(PlayerManager manger)
    {
        pMan = manger;
    }

    #region input Calls
    private void OnPause(InputAction.CallbackContext context)
    {
        if(GameManager.Instance == null)
            return;

        if (!GameManager.Instance.paused)
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

            pMan.pUI.gameObject.SetActive(false);


            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameManager.Instance.pauseMenu.OpenPauseMenu();
            GameManager.Instance.paused = true;
        }
        else
        {
            playerInputMap.Enable();
            shootInputMap.Enable();

            pMan.pUI.gameObject.SetActive(true);


            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.Instance.pauseMenu.ClosePauseMenu();
            GameManager.Instance.paused = false;
        }
   
    }
    public void ToggleUpgradeMenu(bool toggle)
    {
        if (toggle)
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

            pauseGame.performed -= OnPause;
            pauseGame.performed += OnUpgradeMenu;

            pMan.pUI.gameObject.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playerInputMap.Enable();
            shootInputMap.Enable();
            // return to default pause menu interactions
            pauseGame.performed -= OnUpgradeMenu;
            pauseGame.performed += OnPause;

            pMan.pUI.gameObject.SetActive(true);


            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


    }
    private void OnUpgradeMenu(InputAction.CallbackContext context)
    {
        GameManager.Instance.gameUI.ShowUpgradeMenu(false);

        ToggleUpgradeMenu(false);
    }
    private void OnExitGame(InputAction.CallbackContext context)
    {
        Debug.Log("Closing down Game");
        Application.Quit();
    }
    private void OnLeaveGame(InputAction.CallbackContext context)
    {
        GameManager.Instance.RemovePlayer(pMan);
    }
    private void OnSkipCutscene(InputAction.CallbackContext context)
    {
        GameManager.Instance.waveManager.SkippParkingAnimation();
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
    private void OnToggleShowRadius(InputAction.CallbackContext context)
    {
        pMan.bGun.ToggleShowRadius();
    }
    private void OnAimSightCall(InputAction.CallbackContext context)
    {
        pMan.pAnime.ToggleAimSightAnime(true);
        pMan.pCamera.AimSight();
    }
    private void OnDefaultSightCall(InputAction.CallbackContext context)
    {
        pMan.pAnime.ToggleAimSightAnime(false);
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
        if (pMan.currentWeapon != null)
        {
            pMan.currentWeapon.StartReload();
        }
    }
    private void OnFlipCamera(InputAction.CallbackContext context)
    {
        pMan.pCamera.FlipCameraSide();
    }
    private void OnProgressTutorial(InputAction.CallbackContext context)
    {
        if(Tutorial.Instance == null)
            return;
        if (Tutorial.Instance.clickNeeded == true)
        {
            shootInputMap.Disable();
            Tutorial.Instance.ProgressTutorial();
            shootInputMap.Enable();
        }
    }

    // building stuff
    private void OnBuildMode(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon == null)
            return;

        if (!pMan.isBuilding)
        {
            // if player is holding fire, prevent press from carrying over
            fireAction.Disable();
            //Change build Actions
            fireAction = placementInputMap.FindAction("WideView");
            
            // Assign build mode actions
            fireAction.started += OnFireStarted;
            fireAction.canceled += OnFireCanceled;
            
            pMan.ToggleBuildMode();

            shootInputMap.Disable();
            editInputMap.Disable();
            buildInputMap.Enable();
            placementInputMap.Enable();
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
            placementInputMap.Disable();
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
            buildGun.CycleBuildableStructure(changeStructAction.ReadValue<float>()); // Cycle through build able objects
            pMan.pUI.AmmoDisplay(pMan.currentWeapon);
        }
    }
    private void OnPlaceStructure(InputAction.CallbackContext context)
    {
        pMan.bGun.PlaceStructure();
    }
    // Editing a Structure
    private void OnToggleEditMode(InputAction.CallbackContext context)
    {
        if (!pMan.bGun.isEditing)
        {
            EnterEditMode();
        }
        else
        {
            ExitEditMode();
        }
    }
    private void EnterEditMode()
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.EnterEditMode();

            pMan.pUI.EnableControls("<color=green>Edit Mode</color> \n Left mouse to Move \n Hold X to Destroy \n F to return");
            placementInputMap.Disable();
            editInputMap.Enable();
        }
    }
    private void ExitEditMode()
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.ExitEditMode();
           
            pMan.pUI.EnableControls("<color=red>Build Mode</color> \n Mousewheel to change Structure \n Hold Right mouse to Preview \n F to enter Edit Mode");
            editInputMap.Disable();
            placementInputMap.Enable();
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
            buildGun.SellStructure();
        }
    }
    private void OnRotateStarted(InputAction.CallbackContext context)
    {
        if( rotateStructAction.ReadValue<float>() != 0)
            rotationDirection = rotateStructAction.ReadValue<float>();
    }
    private void OnRotate(InputAction.CallbackContext context)
    {
        if (context.interaction is HoldInteraction)
        {
            StartCoroutine(KeepRotating());
        }
        else
        {
            pMan.bGun.RotateStructure(rotationDirection);
        }

    }
    IEnumerator KeepRotating()
    {
        while (rotateStructAction.IsPressed())
        {
            pMan.bGun.RotateStructure(rotateStructAction.ReadValue<float>() * 10 * Time.deltaTime);
            yield return null;
        }
    }
    public string GetInteractionKey()
    {
        string key = interactAction.GetBindingDisplayString();

        return key;
    }

    #endregion
}
