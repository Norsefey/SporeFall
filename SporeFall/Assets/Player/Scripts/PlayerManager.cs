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
    [SerializeField] private Transform fallbackSpawnPoint;
    public bool isRespawning = false;

    public bool holdingCorruption = false;
    public InputDevice myDevice;

    [Header("Toxic Water")]
    [SerializeField] private float toxicDamageRate = 1;
    public float slowDownMultiplier = .25f;
    public bool inToxicWater;

    public Coroutine respawnCoroutine;

    private void Awake()
    {
        pInput = GetComponent<PlayerInputOrganizer>();
        // since we will have multiple players, this manager cannot be a public instance, so we assign it locally
        SetManager();
        SetDeviceSettings();
        // To prevent player from seeing spawning stuff, or switching camera too soon
        TogglePCamera(false);

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

        if (Input.GetKey(KeyCode.J) && (Input.GetKeyDown(KeyCode.K)))
        {
            defaultWeapon.damage = 1000;
            defaultWeapon.hitScanDistance = 500;
            defaultWeapon.bulletCount = 99999;
            pHealth.canTakeDamage = false;
            pHealth.canHoldCorruption = false;
            GameManager.Instance.IncreaseMycelia(9999);
            GameManager.Instance.trainHandler.trainHP.canTakeDamage = false;
        }

       /* // For Testing
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
                    pUI.ToggleWeaponUI(false);
                }
                else
                {
                    meleeActive = false;

                    currentWeapon.gameObject.SetActive(false);

                    EquipDefaultGun();
                }

             
            }
        }*/
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
        currentWeapon.gameObject.SetActive(true);
        nearByWeapon.SetActive(false);
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

        if(currentWeapon is ChargeGun)
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
            /*if(Tutorial.Instance != null)
            {
                if (Tutorial.Instance.currentScene == "Tutorial")
                {
                    Tutorial.Instance.ProgressTutorial();
                }
            }*/
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
        pUI.ToggleWeaponUI(true);

    }
    #region Toggle Switches
    public void ToggleBuildMode()
    {
        if(currentWeapon == null)
            return;

        if (!isBuilding && canBuild)
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
            if (Tutorial.Instance != null && Tutorial.Instance.currentScene == "Tutorial" && Tutorial.Instance.tutorialPrompt == 15)
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

        if (!toggle)
        {
            pController.currentState = PlayerMovement.PlayerState.Immobile;
        }
        else
        {
            pController.currentState = PlayerMovement.PlayerState.Default;
        }
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
        Debug.Log("Moving player to: " + position);
        pController.transform.position = position;

        Debug.Log("Player At: " + pController.transform.position);


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
        if (!isRespawning) // Add a flag to prevent multiple respawn processes
        {
            isRespawning = true;
            respawnCoroutine = StartCoroutine(Respawn(waitTime, resetStats));
        }
    }
    private IEnumerator Respawn(float waitTime, bool resetStats)
    {
        Debug.Log("Starting Player Respawn");

        // Clean up current state
        DropWeapon();
        if (isBuilding)
        {
            ToggleBuildMode();
        }

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
            Debug.Log("Continuing respawn process");
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
                spawnPosition = fallbackSpawnPoint != null ? fallbackSpawnPoint.position : transform.position;
            }

            // Move player to spawn position
            MovePlayerTo(spawnPosition);
            TogglePVisual(true);

            yield return new WaitForSecondsRealtime(.2f);

            // Reset player state
            Debug.Log("Resetting player state and returning control");

            TogglePControl(true);
            TogglePCorruption(true);

            pAnime.ToggleIKAim(true);
            pAnime.ToggleRespawn(false);
            pAnime.SetWeaponHoldAnimation(currentWeapon.holdType);

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
