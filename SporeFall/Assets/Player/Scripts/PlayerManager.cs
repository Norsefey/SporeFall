// Ignore Spelling: mycelia Interactable

using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public ChangePlayerMaterials changePlayerMaterials;
    public ApplyColors coloring;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip health75Sound;
    public AudioClip health50Sound;
    public AudioClip health25Sound;
    public AudioClip deathSound;

    [Header("Corruption Audio")]
    public AudioClip corruption25Sound;
    public AudioClip corruption50Sound;
    public AudioClip corruption75Sound;
    public AudioClip corruption100Sound;

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
    public bool canBuild = true;
    private bool canUseWeapon = true;

    [Header("Respawn")]
    [SerializeField] private float respawnTime;
    public bool isRespawning = false;

    public bool holdingCorruption = false;
    public InputDevice myDevice;

    [Header("Toxic Water")]
    [SerializeField] private float toxicDamageRate = 1;
    public float slowDownMultiplier = .25f;
    public bool inToxicWater;

    [HideInInspector] public string playerDevice;
    public Coroutine respawnCoroutine;

    private bool godMode = false;
    private bool p1ControlsSet = false;
    private bool p2ControlsSet = false;
    private void Awake()
    {
        pInput = GetComponent<PlayerInputOrganizer>();
        // since we will have multiple players, this manager cannot be a public instance, so we assign it locally
        SetManager();
        SetDeviceSettings();
        // To prevent player from seeing spawning stuff, or switching camera too soon
        TogglePCamera(false);

        if(TutorialControls.Instance != null)
            TutorialControls.Instance.playerActive = true;
            //Debug.Log("Telling tutorial player is active");
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
        if(canUseWeapon)
            WeaponBehavior();

        if(inToxicWater)
        {
            pHealth.TakeDamage(toxicDamageRate * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            StartRespawn(1, true);
        }

        if (Input.GetKey(KeyCode.J))
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (!godMode)
                {
                    defaultWeapon.damage = 1000;
                    defaultWeapon.hitScanDistance = 500;
                    defaultWeapon.bulletCount = 99999;
                    pHealth.canTakeDamage = false;
                    pHealth.canHoldCorruption = false;
                    GameManager.Instance.IncreaseMycelia(9999);
                    if(GameManager.Instance.trainHandler != null)
                    {
                        GameManager.Instance.trainHandler.trainHP.canTakeDamage = false;
                        GameManager.Instance.trainHandler.UI.ChangeHPDisplay("DOGMODE");
                    }
                    godMode = true;
                }
                else
                {
                    defaultWeapon.damage = 10;
                    defaultWeapon.hitScanDistance = 50;
                    defaultWeapon.bulletCount = 15;
                    pHealth.canTakeDamage = true;
                    pHealth.canHoldCorruption = true;
                    GameManager.Instance.DecreaseMycelia(9999);
                    if (GameManager.Instance.trainHandler != null)
                    {
                        GameManager.Instance.trainHandler.trainHP.canTakeDamage = true;
                        GameManager.Instance.trainHandler.trainHP.TakeDamage(0);
                    }
                    godMode = false;
                }
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                GameManager.Instance.waveManager.KillALLEnemies();
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
        Debug.Log("Setting device settings");
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput.devices.Count > 0)
        {
            var device = playerInput.devices[0];
            myDevice = device;
            if (device is Gamepad)
            {
                playerDevice = "Gamepad";
                Debug.Log("I am using a gamepad");
                pCamera.SetGamepadSettings();
                bGun.structRotSpeed = 50;

                int playerIndex = GetPlayerIndex();
                if (TutorialControls.Instance != null)
                {
                    if (playerIndex == 0 && p1ControlsSet == false)
                    {
                        Debug.Log("Setting xbox controls p1");
                        TutorialControls.Instance.SetXboxInputsP1();
                        p1ControlsSet = true;
                    }

                    else if (playerIndex == 1 && p2ControlsSet == false)
                    {
                        Debug.Log("Setting xbox controls p2");
                        TutorialControls.Instance.SetXboxInputsP2();
                        p2ControlsSet = true;
                    }
                }

            }
            else if (device is Keyboard || device is Mouse)
            {
                playerDevice = "Mouse";
                //Debug.Log("I am using a keyboard");
                pCamera.SetMouseSettings();
                bGun.structRotSpeed = 25;

                int playerIndex = GetPlayerIndex();
                if (TutorialControls.Instance != null)
                {
                    if (playerIndex == 0 && p1ControlsSet == false)
                    {
                        //Debug.Log("Setting keyboard controls p1");
                        TutorialControls.Instance.SetKeyboardInputsP1();
                        p1ControlsSet = true;
                    }

                    else if (playerIndex == 1 && p2ControlsSet == false)
                    {
                        //Debug.Log("Setting keyboard controls p2");
                        TutorialControls.Instance.SetKeyboardInputsP2();
                        p2ControlsSet = true;
                    }
                    
                }
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
            else if (currentWeapon is ChargeGun gun)
            {
                if (isCharging)
                {
                    // Charge weapons handle firing when the fire button is held
                    gun.Charge();
                    pUI.UpdateChargeGunSlider(gun.chargeAmount);
                }
                else
                {
                    pUI.UpdateChargeGunSlider(0);
                }

            }
        }
    }
    public void PickUpWeapon()
    {
        if (nearByWeapon == null)
            return;
        EquipNewGun(nearByWeapon);

        // disable pick up
        nearByWeapon.SetActive(false);
        pUI.DisablePrompt(); 
    }
    public void DestroyCurrentWeapon()
    {
        if (currentWeapon != null)
        {
            if (currentWeapon.IsReloading)
                currentWeapon.CancelReload();

            if (currentWeapon is ChargeGun)
            {
                pUI.ToggleChargeGunSlider(false);
            }
            Destroy(currentWeapon.gameObject); // Drop the current weapon
        }

        currentWeapon = null;
        equippedWeapon = null;
        // reactivate the default weapon
        pUI.AmmoDisplay(currentWeapon);
        pUI.SwitchWeaponIcon();
        pAnime.SetWeaponHoldAnimation(0);
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

            if(currentWeapon is ChargeGun)
            {
                pUI.ToggleChargeGunSlider(false);
            }
            Destroy(currentWeapon.gameObject); // Drop the current weapon
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
    public void EquipNewGun(GameObject newWeapon)
    {
        // Avoid getting Stuck in reload
        if (currentWeapon != null && currentWeapon.IsReloading)
            currentWeapon.CancelReload();
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
        currentWeapon = Instantiate(newWeapon, weaponHolder).GetComponent<Weapon>();
        // set the transforms of the new weapon
        currentWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        currentWeapon.gameObject.SetActive(true);

        // set References
        currentWeapon.player = this;
        equippedWeapon = currentWeapon;
        // update UI to display new ammo capacities
        pUI.AmmoDisplay(currentWeapon);
        // update weapon icon
        pUI.SwitchWeaponIcon();

        // Charge Guns use an additional UI Element
        if (currentWeapon is ChargeGun)
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
        pUI.ToggleWeaponUI(true);
    }
    public void EquipDefaultGun()
    {
        currentWeapon = defaultWeapon;
        currentWeapon.gameObject.SetActive(true);
        pUI.SwitchWeaponIcon();
        pAnime.SetWeaponHoldAnimation(currentWeapon.holdType);
        pUI.ToggleWeaponUI(true);

    }
    #region Toggle Switches
    public void ToggleBuildMode(bool toggle)
    {
        if(currentWeapon == null)
            return;

        if (toggle && canBuild)
        {// Enter Build mode
         // Avoid getting Stuck in reload
            if (currentWeapon.IsReloading)
                currentWeapon.CancelReload();
            currentWeapon.gameObject.SetActive(false);
            bGun.gameObject.SetActive(true);
            currentWeapon = bGun;
            pUI.buildUI.SetActive(true);

            int playerIndex = GetPlayerIndex();
            //Debug.Log("Player index = " + playerIndex);
            if (playerIndex == 0)
            {
                pUI.EnableControls("<color=yellow>Build Mode</color> \n " + TutorialControls.Instance.scrollInput + " to change Structure \n Hold " + TutorialControls.Instance.aimInput + " to Preview \n " + TutorialControls.Instance.editInput + " to enter Edit Mode");
            }

            else if (playerIndex == 1)
            {
                pUI.EnableControls("<color=yellow>Build Mode</color> \n " + TutorialControls.Instance.scrollInput2 + " to change Structure \n Hold " + TutorialControls.Instance.aimInput2 + " to Preview \n " + TutorialControls.Instance.editInput2 + " to enter Edit Mode");
            }
            
            pUI.AmmoDisplay(currentWeapon);
            pUI.SwitchWeaponIcon();
            pAnime.SetWeaponHoldAnimation(1);
            isBuilding = true;
            if (Tutorial.Instance != null && SavedSettings.currentLevel == "Tutorial" && Tutorial.Instance.tutorialPrompt == 15)
            {
                Tutorial.Instance.ProgressTutorial();
            }
        }
        else if(!toggle)
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
            ToggleBuildMode(false);

        pController.gameObject.SetActive(toggle);
        canUseWeapon = toggle;
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
        //Debug.Log("Moving player to: " + position);
        pController.transform.position = position;

        //Debug.Log("Player At: " + pController.transform.position);


        // Verify the move was successful
        if (Vector3.Distance(pController.transform.position, position) > 1f)
        {
            Debug.LogError("Player did not move to spawn position correctly \n Player At " + pController.transform.position + " Corrected to- " + position);
            // Force position directly as fallback
            pController.transform.position = position;
        }
    }
    public void StartRespawn(float waitTime, bool resetStats)
    {
        if (!isRespawning) // to prevent multiple respawn processes
        {
            isRespawning = true;
            respawnCoroutine = StartCoroutine(Respawn(waitTime, resetStats));
        }
    }
    private IEnumerator Respawn(float waitTime, bool resetStats)
    {
        //Debug.Log("Starting Player Respawn");

        if (isBuilding)
        {
            ToggleBuildMode(false);
        }
        // Clean up current state
        DropWeapon();
        // Disable player control during respawn
        TogglePControl(false);

        // Use unscaled time if there's a chance Time.timeScale could be 0
        yield return new WaitForSecondsRealtime(waitTime);

        if (pHealth.CurrentLives <= 0)
        {
            Debug.Log("No lives remaining, respawn canceled");
            // Handle game over or player death permanently
        }
        else
        {
            pAnime.ToggleRespawn(true);

            // Determine spawn position with fallback
            Vector3 spawnPosition;
            Transform spawnPoint = null;

            if (GameManager.Instance != null && GameManager.Instance.trainHandler != null)
            {
                int playerIndex = GetPlayerIndex();
                if (playerIndex >= 0 && playerIndex < GameManager.Instance.trainHandler.playerSpawnPoint.Length)
                {
                    spawnPoint = GameManager.Instance.trainHandler.playerSpawnPoint[playerIndex];
                    Debug.Log("Using train spawn point" + spawnPoint.name + " for player " + playerIndex);
                }
            }

            if (spawnPoint != null)
            {
                spawnPosition = spawnPoint.position;
            }
            else
            {
                Debug.LogWarning("Train spawn point not available, using fallback");
                spawnPosition = GameManager.Instance.backUpPlayerSpawner.playerSpawnPoints[0].position;
            }

            // Move player to spawn position
            MovePlayerTo(spawnPosition);

            yield return new WaitForSecondsRealtime(.2f);

            // Reset player state
            Debug.Log("Resetting player state and returning control");
            TogglePVisual(true);

            TogglePControl(true);
            TogglePCorruption(true);

            pAnime.ToggleIKAim(true);
            pAnime.ToggleRespawn(false);
            if(currentWeapon != null)
                pAnime.SetWeaponHoldAnimation(currentWeapon.holdType);
            else 
                pAnime.SetWeaponHoldAnimation(0);



            if (resetStats)
            {
                pCorruption.ResetCorruptionLevel();
                pHealth.ResetHealth();
            }
           
        }
        isRespawning = false;
    }
    public void SetupPlayerTwo()
    {
        pCamera.DisableAudioListener();
        changePlayerMaterials.ChangeMaterials();
        pHealth.SetReducedLife();
    }
    public int GetPlayerIndex()
    {
        return GetComponent<PlayerInput>().playerIndex;
    }
}
