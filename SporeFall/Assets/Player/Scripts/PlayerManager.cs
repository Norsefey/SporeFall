// Ignore Spelling: mycelia Interactable

using System.Collections;
using System.Collections.Generic;
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
    public CorruptionHandler pCorruption;
    public TrainHandler train;
    //public WaveManager waveManager;
    private Interactables interactable;
    [Header("Weapons")]
    // Weapons and Shooting
    public Transform weaponHolder; // Where the weapon is equipped
    public Weapon currentWeapon;
    public Weapon defaultWeapon;
    public Weapon equippedWeapon;
    // pickables
    public GameObject nearByPickUp;

    public bool isFiring = false;
    public bool isCharging = false;
    public bool isBuilding = false;
    public bool isRotating = false;
    [Header("Currency")]
    // Player Stats
    public float mycelia = 30;
 
    [Header("Respawn")]
    [SerializeField] private float respawnTime;

    public bool holdingCorruption = false;
    public InputDevice myDevice;
    private void Awake()
    {
        pInput = GetComponent<PlayerInputOrganizer>();
        // since we will have multiple players, this manager cannot be a public instance, so we assign it locally
        pInput.SetManager(this);
        pController.SetManager(this);
        pCamera.SetManager(this);
        pCorruption.SetManager(this);
        pHealth.SetManager(this);
        pUI.SetManager(this);
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
            else if (device is Keyboard)
            {
                Debug.Log("I am using a keyboard");
                pCamera.SetMouseSettings();
                bGun.structRotSpeed = 25;
            }
        }
    }
    private void Start()
    {
        if(WaveManager.Instance != null)
            WaveManager.Instance.train.AddPlayer(this);
        // in order to spawn player at a spawn point, disable movement controls
        pUI.AmmoDisplay(currentWeapon);
        pInput.AssignAllActions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        if (currentWeapon != null)
        {
            if (currentWeapon is BuildGun bGun)
            {
                pUI.DisplayMycelia(mycelia);
                if (bGun.isEditing)
                {
                    if (bGun.SelectStructure())
                        bGun.RotateStructure();
                }
            }

            if (isFiring && !currentWeapon.IsReloading && currentWeapon is not ChargeGun)
            {
                currentWeapon.Fire();
                pUI.AmmoDisplay(currentWeapon);
            }
            else if (isCharging && (currentWeapon is ChargeGun gun))
            {
                // Charge weapons handle firing when the fire button is held
                gun.Charge();
            }
        }
    }
    public void ToggleBuildMode()
    {
        if (!isBuilding)
        {// Enter Build mode
            currentWeapon.gameObject.SetActive(false);
            bGun.gameObject.SetActive(true);
            currentWeapon = bGun;
            pUI.EnablePrompt("<color=red>Build Mode</color> \nUse Q/E to change Structure" + "\n F to Select Structure" + "\n Hold Right mouse to Preview");
            pUI.AmmoDisplay(currentWeapon);
            isBuilding = true;
        }
        else
        {// Exit Build Mode

            if (bGun.isEditing)
            {
                bGun.ExitEditMode();
            }
            else
            {
                bGun.DestroySelectedObject();
            }

            if (equippedWeapon != null)
                currentWeapon = equippedWeapon;
            else
                currentWeapon = defaultWeapon;

            bGun.gameObject.SetActive(false);
            currentWeapon.gameObject.SetActive(true);
            pUI.AmmoDisplay(currentWeapon);
            pUI.DisablePrompt();
            isFiring = false;
            isBuilding = false;
        }
    }
    public void PickUpWeapon()
    {
        if (nearByPickUp == null)
            return;
        // deactivate the default weapon
        if (currentWeapon == defaultWeapon || currentWeapon == bGun)
        {
            currentWeapon.gameObject.SetActive(false);
        }
        else if (equippedWeapon != null)
        {
            Destroy(currentWeapon.gameObject); // Drop the current weapon
        }
        // Equip the new weapon
        currentWeapon = Instantiate(nearByPickUp, weaponHolder).GetComponent<Weapon>();
        // set the transforms of the new weapon
        currentWeapon.transform.forward = pController.transform.forward;
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.player = this;
        equippedWeapon = currentWeapon;
        // destroy pick up platform
        Destroy(nearByPickUp.transform.parent.gameObject);
        // disable pick up prompt
        pUI.DisablePrompt();
        // update UI to display new ammo capacities
        pUI.AmmoDisplay(currentWeapon);
        Debug.Log("Picked up: " + currentWeapon.weaponName);

        if (currentWeapon.isCorrupted)
            holdingCorruption = true;
    }
    public void DropWeapon()
    {
        if (equippedWeapon == null || currentWeapon == bGun)
            return;
        Debug.Log("Dropping Weapon");
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject); // Drop the current weapon
        }

        currentWeapon = defaultWeapon;
        equippedWeapon = null;
        // reactivate the default weapon
        currentWeapon.gameObject.SetActive(true);
        pUI.AmmoDisplay(currentWeapon);
        holdingCorruption = false;
    }
    public void TogglePControl(bool toggle)
    {
        pController.gameObject.SetActive(toggle);
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
        pVisual.SetActive(toggle);
    }
    public void MovePlayerTo(Vector3 position)
    {
        pController.transform.localPosition = position;
        Debug.Log("Moving Player");
    }
    public void StartRespawn()
    {
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        TogglePControl(false);
        DropWeapon();
        if (isBuilding)
        {
            ToggleBuildMode();
        }
        Debug.Log("Death Time");
        yield return new WaitForSeconds(2);
        Debug.Log("Respawning");

        MovePlayerTo(train.playerSpawnPoint[GetPlayerIndex()].position);
        pHealth.RestoreHP(pHealth.maxHP);
        TogglePControl(true);
    }
    public void GameOver()
    {
        Debug.Log("You lose");
    }
    public void AssignInteractable(string promptText, Interactables interactable)
    {
        pUI.EnablePrompt(promptText);
        pInput.AssignInteraction(interactable);

        this.interactable = interactable;
    }
    public void RemoveInteractable()
    {
        pUI.DisablePrompt();
        pInput.RemoveInteraction(interactable);

        this.interactable = null;
    }
    public int GetPlayerIndex()
    {
        return GetComponent<PlayerInput>().playerIndex;
    }
}
