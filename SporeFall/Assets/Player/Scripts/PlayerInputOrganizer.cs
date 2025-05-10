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
    public InputAction jumpAction;
    public InputAction aimAction;
    public InputAction sprintAction;
    public InputAction interactAction;
    private InputAction flipCameraSide;
    public InputAction buildModeAction;
    private InputAction tutorialAction;
    // Shoot Actions
    private InputAction fireAction;
    private InputAction reloadAction;
    public InputAction dropAction;
    // Build Actions
    private InputAction showStructureRadius;
    public InputAction editModeAction;
    private InputAction rotateStructAction;
    // Placement Actions
    private InputAction hotKeySelection;
    public InputAction changeStructAction;
    private InputAction placeStructAction;
    // Edit Actions
    private InputAction moveStructAcion;
    public InputAction destroyStructAction;
    // have to store this, since Hold needs to be check first, and after that, release value is just zero
    private float rotationDirection = 0;
    private float holdDuration = 0.6f; // Use duration of hold from InputAction asset
    private float currentHoldTime = 0f;
    private bool holdingSell = false;

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
        jumpAction = playerInputMap.FindAction("Jump");
        // shoot action map
        reloadAction = shootInputMap.FindAction("Reload");
        dropAction = shootInputMap.FindAction("Drop");
        fireAction = shootInputMap.FindAction("Fire");
        // build Action Maps
        showStructureRadius = buildInputMap.FindAction("ToggleRadius");
        rotateStructAction = buildInputMap.FindAction("Rotate");
        editModeAction = buildInputMap.FindAction("EditMode");
        // Placement Action map
        hotKeySelection = placementInputMap.FindAction("HotKey");
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
        if (GameManager.Instance.waveManager != null)
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
        hotKeySelection.performed += OnHotKeyPressed;
        changeStructAction.performed += OnCycleBuildStrcuture;
        placeStructAction.performed += OnPlaceStructure;
        // Edit Actions
        moveStructAcion.started += OnEditStructureMoveStarted;
        moveStructAcion.canceled += OnEditMoveStructureCanceled;
        
        destroyStructAction.started += OnEditSellStarted;
        destroyStructAction.canceled += OnEditSellCanceled;
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
        hotKeySelection.performed -= OnHotKeyPressed;

        // edit Actions
        moveStructAcion.started -= OnEditStructureMoveStarted;
        moveStructAcion.canceled -= OnEditMoveStructureCanceled;
        destroyStructAction.started -= OnEditSellStarted;
        destroyStructAction.canceled -= OnEditSellCanceled;
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
        Debug.Log("Interaction Removed");
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

                pMan.ToggleBuildMode(false);

                editInputMap.Disable();
                placementInputMap.Disable();
                buildInputMap.Disable();
                shootInputMap.Enable();
                // After Shoot map is enabled re enable fire action
                fireAction.Enable();
            }

            playerInputMap.Disable();
            shootInputMap.Disable();

            pMan.pUI.gameObject.SetActive(false);
            GameManager.Instance.gameUI.ToggleTutorialPrompts(true);
            if (GameManager.Instance.trainHandler != null)
                GameManager.Instance.trainHandler.UI.gameObject.SetActive(false);
            
            if (GameManager.Instance.waveManager != null && SavedSettings.currentLevel != "Training")
            {
                GameManager.Instance.waveManager.wUI.gameObject.SetActive(false);
            }
            GameManager.Instance.gameUI.gameplayUI.SetActive(false);


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
            GameManager.Instance.gameUI.ToggleTutorialPrompts(false);
            if (GameManager.Instance.trainHandler != null)
                GameManager.Instance.trainHandler.UI.gameObject.SetActive(true);

            if (GameManager.Instance.waveManager != null && SavedSettings.currentLevel != "Training")
            {
                GameManager.Instance.waveManager.wUI.gameObject.SetActive(true);
            }
            GameManager.Instance.gameUI.gameplayUI.SetActive(true);

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

                pMan.ToggleBuildMode(false);

                editInputMap.Disable();
                placementInputMap.Disable();
                buildInputMap.Disable();
                shootInputMap.Enable();
                // After Shoot map is enabled re enable fire action
                fireAction.Enable();
            }

            playerInputMap.Disable();
            shootInputMap.Disable();

            pauseGame.performed -= OnPause;
            pauseGame.performed += OnCloseUpgradeMenu;

            GameManager.Instance.gameUI.ToggleUpgradeMenu(true, pMan);
            GameManager.Instance.gameUI.ToggleTutorialPrompts(true);
            GameManager.Instance.gameUI.gameplayUI.SetActive(false);

            pMan.pUI.gameObject.SetActive(false);
            
            if (GameManager.Instance.trainHandler != null)
                GameManager.Instance.trainHandler.UI.gameObject.SetActive(false);

            if (GameManager.Instance.waveManager != null && SavedSettings.currentLevel != "Training")
            {
                GameManager.Instance.waveManager.wUI.gameObject.SetActive(false);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playerInputMap.Enable();
            shootInputMap.Enable();
            // return to default pause menu interactions
            pauseGame.performed -= OnCloseUpgradeMenu;
            pauseGame.performed += OnPause;

            GameManager.Instance.gameUI.ToggleUpgradeMenu(false, pMan);
            GameManager.Instance.gameUI.ToggleTutorialPrompts(false);
            GameManager.Instance.gameUI.gameplayUI.SetActive(true);

            pMan.pUI.gameObject.SetActive(true);
            if(GameManager.Instance.trainHandler != null)
                GameManager.Instance.trainHandler.UI.gameObject.SetActive(true);
            
            if (GameManager.Instance.waveManager != null && SavedSettings.currentLevel != "Training")
            {
                GameManager.Instance.waveManager.wUI.gameObject.SetActive(true);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    private void OnCloseUpgradeMenu(InputAction.CallbackContext context)
    {
        GameManager.Instance.gameUI.ToggleGameUI(true);
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
        
        if (Tutorial.Instance != null)
        {
            Tutorial.Instance.timer = 20f;
        }
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
        // Check if the jump action is started (button pressed down)
        if (context.started)
        {
            // Call the jump method on the player controller
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
        if (Tutorial.Instance.clickNeeded == true && Tutorial.Instance.tutorialPopup.activeInHierarchy)
        {
            shootInputMap.Disable();
            Tutorial.Instance.ProgressTutorial();
            shootInputMap.Enable();
        }
    }

    // building stuff
    private void OnHotKeyPressed(InputAction.CallbackContext context)
    {
        pMan.bGun.SelectStructureHotKey((int)context.ReadValue<float>());
    }
    private void OnBuildMode(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon == null || !pMan.canBuild)
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
            
            pMan.ToggleBuildMode(true);

            shootInputMap.Disable();
            editInputMap.Disable();
            buildInputMap.Enable();
            placementInputMap.Enable();
            // After Build map is enabled re enable fire action
            fireAction.Enable();

            pMan.pUI.DisplayBuildPanel();
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

            pMan.ToggleBuildMode(false);

            editInputMap.Disable();
            placementInputMap.Disable();
            buildInputMap.Disable();
            shootInputMap.Enable();
            // After Shoot map is enabled re enable fire action
            fireAction.Enable();
            pMan.pUI.DisplayDefaultPanel();
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
            int playerIndex = pMan.GetPlayerIndex();
            if (playerIndex == 0)
            {
                pMan.pUI.EnableControls("<color=blue>Edit Mode</color> \n Hold " + TutorialControls.Instance.shootInput + " to Move Structure \n Hold " + TutorialControls.Instance.destroyInput + " to Destroy \n " + TutorialControls.Instance.editInput + " to return");
            }

            else if (playerIndex == 1)
            {
                pMan.pUI.EnableControls("<color=blue>Edit Mode</color> \n Hold " + TutorialControls.Instance.shootInput2 + " to Move Structure \n Hold " + TutorialControls.Instance.destroyInput2 + " to Destroy \n " + TutorialControls.Instance.editInput2 + " to return");
            }

            pMan.pUI.DisplayEditPanel();
            placementInputMap.Disable();
            editInputMap.Enable();
        }
    }
    private void ExitEditMode()
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.ExitEditMode();
            int playerIndex = pMan.GetPlayerIndex();
            Debug.Log("Player index = " + playerIndex);
            if (playerIndex == 0)
            {
                pMan.pUI.EnableControls("<color=yellow>Build Mode</color> \n " + TutorialControls.Instance.scrollInput + " to change Structure \n Hold " + TutorialControls.Instance.aimInput + " to Preview \n " + TutorialControls.Instance.editInput + " to enter Edit Mode");
            }

            else if (playerIndex == 1)
            {
                pMan.pUI.EnableControls("<color=yellow>Build Mode</color> \n " + TutorialControls.Instance.scrollInput2 + " to change Structure \n Hold " + TutorialControls.Instance.aimInput2 + " to Preview \n " + TutorialControls.Instance.editInput2 + " to enter Edit Mode");
            }
            
            pMan.pUI.DisplayBuildPanel();
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
    private void OnEditMoveStructureCanceled(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.OnFireReleased();
            pMan.isFiring = false;
        }
    }
    private void OnEditSellStarted(InputAction.CallbackContext context)
    {
        if(pMan.bGun.selectedStructure == null)
            return;
        holdingSell = true;
        pMan.pUI.chargeGunSlider.value = 0;
        pMan.pUI.chargeGunSlider.maxValue = 1;

        currentHoldTime = 0;
        StartCoroutine(HoldRoutine());
    }
    private void OnEditSellCanceled(InputAction.CallbackContext context)
    {
        holdingSell = false;
        currentHoldTime = 0;
        pMan.pUI.chargeGunSlider.value = 0;
    }
    private void OnEditDestroy(InputAction.CallbackContext context)
    {
        if (pMan.currentWeapon is BuildGun buildGun)
        {
            buildGun.SellStructure();
        }
        holdingSell = false;
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
    private IEnumerator HoldRoutine()
    {
        while (holdingSell)
        {
            currentHoldTime += Time.deltaTime;
            // Calculate how far to fill the slider
            float sliderValue = Mathf.Clamp01(currentHoldTime / holdDuration);
            pMan.pUI.chargeGunSlider.value = sliderValue;
            yield return null;
        }
        yield return new WaitForSeconds(1);
        pMan.pUI.chargeGunSlider.value = 0;
    }
    #endregion
}
