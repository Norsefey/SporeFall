using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public PlayerMovement pController;
    [SerializeField] public TPSCamera pCamera;
    [SerializeField] public PlayerUI pUI;
    [SerializeField] public GameObject pVisual;
    
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
    public Weapon currentWeapon;
    public Transform weaponHolder; // Where the weapon is equipped
    private bool isFiring = false;
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
        pUI.AmmoDisplay(currentWeapon.magazineCount, currentWeapon.totalAmmo);

    }

    private void Update()
    {
        if (currentWeapon != null)
        {
            if (isFiring && !currentWeapon.isReloading)
            {
                currentWeapon.Fire();
                pUI.AmmoDisplay(currentWeapon.magazineCount, currentWeapon.totalAmmo);
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
        fireAction.started += OnFire;
        fireAction.canceled += OnFire;
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
        fireAction.started -= OnFire;
        fireAction.canceled -= OnFire;
        reloadAction.performed -= OnReload;

        // disable Input map
        player.Disable();
    }
    // Called when the Fire action is triggered
    private void OnFire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            isFiring = true;
            pController.SetAimState();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isFiring = false;
            if(!aimAction.IsPressed())
                pController.SetDefaultState();
        }
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
        pUI.AmmoDisplay(currentWeapon.magazineCount, currentWeapon.totalAmmo);

        Debug.Log("Picked up: " + currentWeapon.weaponName);
    }
}
