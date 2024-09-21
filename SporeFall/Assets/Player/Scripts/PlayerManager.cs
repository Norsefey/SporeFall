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
    public BuildGun bGun;
    /// <summary>
    ///  Might have to move these inputs to their own script, cause they are growing and needlessly expanding this script
    /// </summary>
    // Input Maps
    private InputActionAsset inputAsset;
    private InputActionMap playerInputMap;
    private InputActionMap shootInputMap;
    private InputActionMap buildInputMap;
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
    [Header("Weapons")]
    // Weapons and Shooting
    public Transform weaponHolder; // Where the weapon is equipped
    public Weapon currentWeapon;
    public Weapon defaultWeapon;
    public Weapon equippedWeapon;
    private GameObject nearByWeapon;
    private bool isFiring = false;
    private bool isCharging = false;
    private bool isBuilding = false;
    private void Awake()
    {
        // get and assign input Action Map
        inputAsset = this.GetComponent<PlayerInput>().actions;
        playerInputMap = inputAsset.FindActionMap("Player");
        shootInputMap = inputAsset.FindActionMap("Shoot");
        buildInputMap = inputAsset.FindActionMap("Build");
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
        // Assign Calls to each action
        // basic player actions
        jumpAction.started += pController.JumpCall;
        aimAction.started += pCamera.AimSightCall;
        aimAction.canceled += pCamera.DefaultSightCall;
        fireAction.started += OnFireStarted;
        fireAction.canceled += OnFireCanceled;
        buildModeAction.started += OnBuildMode;
        // shoot actions
        reloadAction.performed += OnReload;
        pickUpAction.performed += OnPickUpWeapon;
        // build mode actions
        changeBuildAction.started += OnCycleBuildObject;
        placeBuildAction.started += OnPlaceObject;

        // this enables the controls
        playerInputMap.Enable();
        shootInputMap.Enable();
        buildInputMap.Disable();
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
        buildModeAction.started -= OnBuildMode;
        changeBuildAction.started -= OnCycleBuildObject;
        placeBuildAction.started -= OnPlaceObject;

        // disable Input map
        playerInputMap.Disable();
        shootInputMap.Disable();
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
        switch (currentWeapon)
        {
            case ChargeGun:
                isCharging = false; // Fire the charged shot when fire button is released
                ((ChargeGun)currentWeapon).Release();
                pUI.AmmoDisplay(currentWeapon);
                break;
            case BurstGun:
                ((BurstGun)currentWeapon).OnFireReleased(); // Call the release method on the burst weapon
                break;
            case BuildGun:
                ((BuildGun)currentWeapon).OnFireReleased(); // Call the release method on the burst weapon
                break;
            default:
                break;
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
    // building stuff
    private void OnBuildMode(InputAction.CallbackContext context)
    {
        if (!isBuilding)
        {
            currentWeapon.gameObject.SetActive(false);
            bGun.gameObject.SetActive(true);
            currentWeapon = bGun;
            pUI.AmmoDisplay(currentWeapon);
            buildInputMap.Enable();
            isBuilding = true;
        }
        else
        {
            if(equippedWeapon != null)
                currentWeapon = equippedWeapon;
            else
                currentWeapon = defaultWeapon;

            currentWeapon.gameObject.SetActive(true);
            pUI.AmmoDisplay(currentWeapon);
            bGun.gameObject.SetActive(false);
            buildInputMap.Disable();
            isBuilding = false;
        }
    }
    private void OnCycleBuildObject(InputAction.CallbackContext context)
    {
        if (currentWeapon is BuildGun buildGun)
        {
            buildGun.CycleBuildObject(); // Cycle through buildable objects
            pUI.AmmoDisplay(currentWeapon);

        }
    }
    private void OnPlaceObject(InputAction.CallbackContext context)
    {
        bGun.PlaceObject();
    }
    private void OnSelectObject(InputAction.CallbackContext context)
    {
        if (currentWeapon is BuildGun buildGun)
        {
            buildGun.SelectObject(); // Select a placed object for moving or destroying
        }
    }
    private void OnMoveObject(InputAction.CallbackContext context)
    {
        if (currentWeapon is BuildGun buildGun)
        {
            buildGun.MoveSelectedObject(); // Move the selected object to a new location
        }
    }
    private void OnDestroyObject(InputAction.CallbackContext context)
    {
        if (currentWeapon is BuildGun buildGun)
        {
            buildGun.DestroySelectedObject(); // Destroy the selected object
        }
    }
    private void OnPickUpWeapon(InputAction.CallbackContext context)
    {
        if (nearByWeapon != null)
            PickUpWeapon();
    }
    public void EnablePickUpWeapon(GameObject weapon)
    {
        nearByWeapon = weapon;
        pUI.EnablePromptPickUp(weapon);
    }
    public void DisablePickUpWeapon() 
    { 
        nearByWeapon = null;
        pUI.DisablePromptPickUp();
    }
    // Called when the Reload action is triggered
    private void PickUpWeapon()
    {
         // deactivate the default weapon
         if (currentWeapon == defaultWeapon || currentWeapon == bGun)
        {
            currentWeapon.gameObject.SetActive(false);
        }else if(equippedWeapon != null)
        {
            Destroy(currentWeapon.gameObject); // Drop the current weapon
        }
        // Equip the new weapon
        currentWeapon = Instantiate(nearByWeapon, weaponHolder).GetComponent<Weapon>();
        // set the transforms of the new weapon
        currentWeapon.transform.forward = pController.transform.forward;
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.player = this;
        equippedWeapon = currentWeapon;
        // destroy pick up platform
        Destroy(nearByWeapon.transform.root.gameObject);
        // disable pick up prompt
        pUI.DisablePromptPickUp();
        // update UI to display new ammo capacities
        pUI.AmmoDisplay(currentWeapon);
        Debug.Log("Picked up: " + currentWeapon.weaponName);
    }
    public void DropWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject); // Drop the current weapon
        }

        currentWeapon = defaultWeapon;
        equippedWeapon = null;
        // reactivate the default weapon
        currentWeapon.gameObject.SetActive(true);
        pUI.AmmoDisplay(currentWeapon);
    }

}
