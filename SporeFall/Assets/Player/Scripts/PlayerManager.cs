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
    public PlayerUI pUI;
    public GameObject pVisual;
    public BuildGun bGun;
    [Header("Weapons")]
    // Weapons and Shooting
    public Transform weaponHolder; // Where the weapon is equipped
    public Weapon currentWeapon;
    public Weapon defaultWeapon;
    public Weapon equippedWeapon;
    public GameObject nearByWeapon;
    public bool isFiring = false;
    public bool isCharging = false;
    public bool isBuilding = false;
    private void Awake()
    {
        pInput = GetComponent<PlayerInputOrganizer>();
        // since we will have multiple players, this manager cannot be a public instance, so we assign it locally
        pController.SetManager(this);
        pCamera.SetManager(this);
        pInput.SetManager(this);
    }
    private void Start()
    {
        // in order to spawn player at a spawn point, disable movement controls
        pController.gameObject.SetActive(true);
        pUI.AmmoDisplay(currentWeapon);
        pInput.AssignAllActions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    public void SetBuildMode()
    {
        if (!isBuilding)
        {// Enter Build mode
            currentWeapon.gameObject.SetActive(false);
            bGun.gameObject.SetActive(true);
            currentWeapon = bGun;
            pUI.DisplayAText("Build Mode");
            isBuilding = true;
        }
        else
        {// Exit Build Mode
            if (equippedWeapon != null)
                currentWeapon = equippedWeapon;
            else
                currentWeapon = defaultWeapon;

            currentWeapon.gameObject.SetActive(true);
            pUI.AmmoDisplay(currentWeapon);
            bGun.gameObject.SetActive(false);
            isBuilding = false;
        }
    }
    public void PickUpWeapon()
    {
        if (nearByWeapon == null)
            return;
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
        if(equippedWeapon == null || currentWeapon == bGun)
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
    }
}
