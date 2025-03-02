using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimation : MonoBehaviour
{
    private PlayerManager pMan;

    [SerializeField] private Animator anime;
    [SerializeField] private Transform weaponSlot; // holds weapon
    [SerializeField] private Transform rightHandBone;// where the weapon slot will move to, to avoid having to dig for it in hierarchy
    [SerializeField] private GameObject aimTarget;

    [SerializeField] private Rig headLook;
    // Animation states
    private bool isWalking = false;
    private bool isAiming = false;
    private void Start()
    {

        if (pMan.currentWeapon == null)
        {
            Debug.Log("No Weapon");
            SetWeaponHoldAnimation(0);
            pMan.pUI.ToggleDefaultUI(false);
        }
        else
        {
            Debug.Log("Yes Weapon");
            SetWeaponHoldAnimation(pMan.currentWeapon.holdType);

            pMan.pUI.AmmoDisplay(pMan.currentWeapon);
        }
    }


    // Update is called once per frame
    void Update()
    {
        // keep weapon to hand position
        WeaponPosition();
        WalkCheck();

        if (isAiming)
        {
            pMan.currentWeapon.transform.forward = pMan.pCamera.myCamera.transform.forward;

        }
    }
    private void WeaponPosition()
    {
        if(pMan.currentWeapon == null)
            return;
        weaponSlot.position = rightHandBone.position;
        Vector3 handForward = rightHandBone.up;
        weaponSlot.rotation = Quaternion.LookRotation(handForward);

    }
    private void WalkCheck()
    {
        if (!isWalking && pMan.pInput.moveAction.ReadValue<Vector2>() != Vector2.zero)
        {
            isWalking = true;
            ToggleWalkAnime(true);
            anime.SetFloat("WalkDirection", pMan.pInput.moveAction.ReadValue<Vector2>().y);
        }
        else if (isWalking && pMan.pInput.moveAction.ReadValue<Vector2>() == Vector2.zero)
        {
            isWalking = false;
            ToggleWalkAnime(false);
        }
    }
    public void ToggleWalkAnime(bool toggle)
    {
        anime.SetBool("IsWalking", toggle);
    }
    public void ToggleSprint(bool toggle)
    {
        anime.SetBool("IsSprinting", toggle);
    }
    public void ToggleAimSightAnime(bool toggle)
    {
        if(pMan.currentWeapon == null)
            return;
        anime.SetBool("IsAiming", toggle);
        isAiming = toggle;
    }
    public void ToggleFallingAnime(bool toggle)
    {
        anime.SetBool("IsFalling", toggle);
    }
    public void TriggerAnimation(string trigger)
    {
        anime.SetTrigger(trigger);
    }
    public void SetWeaponHoldAnimation(int weaponType)
    {
        anime.SetInteger("WeaponType", weaponType);

      /*  switch (holdPosIndex)
        {
            case 0:// No Weapon
                Debug.Log("Setting No Weapon Animations");
                tbIk.weight = 0;

                anime.SetBool("NoWeapon", true);
                oneHandedRig.weight = 0;
                //anime.SetLayerWeight(oneHandedLayerIndex, 0);

                twoHandedRig.weight = 0;
                //anime.SetLayerWeight(twoHandedLayerIndex, 0);
                twoHanded = false;
                aimTarget.SetActive(false);

                //anime.SetLayerWeight(noWeaponLayerIndex, 1);
                break;
            case 1:// 1 Handed Weapon
                if (!isAiming)
                    tbIk.weight = 0;

                twoHandedRig.weight = 0;
                oneHandedRig.weight = 1;
                anime.SetLayerWeight(noWeaponLayerIndex, 0);
                anime.SetLayerWeight(twoHandedLayerIndex, 0);

                anime.SetLayerWeight(oneHandedLayerIndex, 1);
                aimTarget.SetActive(true);
                twoHanded = false;
                break;
            case 2:// Two Handed Weapon
                tbIk.weight = 1;

                oneHandedRig.weight = 0;
                twoHandedRig.weight = 1;
                leftHandTarget.position = pMan.currentWeapon.secondHandHold.position;
                anime.SetLayerWeight(noWeaponLayerIndex, 0);
                anime.SetLayerWeight(oneHandedLayerIndex, 0);

                anime.SetLayerWeight(twoHandedLayerIndex, 1);
                aimTarget.SetActive(true);
                twoHanded = true;
                break;


        }*/
    }

    /*public void ToggleTwoHanded(bool toggle)
    {
        if (toggle == twoHanded)// already in state no need to switch
            return;
        if (toggle)
        {
            tbIk.weight = 1;

            oneHandedRig.weight = 0;
            twoHandedRig.weight = 1;
            leftHandTarget.position = pMan.currentWeapon.secondHandHold.position;
            anime.SetLayerWeight(oneHandedLayerIndex, 0);
            anime.SetLayerWeight(twoHandedLayerIndex, 1);
            twoHanded = true;
        }
        else
        {
            if(!isAiming)
                tbIk.weight = 0;

            twoHandedRig.weight = 0;
            oneHandedRig.weight = 1;
            anime.SetLayerWeight(twoHandedLayerIndex, 0);
            anime.SetLayerWeight(oneHandedLayerIndex, 1);
            twoHanded = false;
        }
       
    }*/
    public void ToggleIKAim(bool toggle)
    {
        headLook.weight = toggle ? 1 : 0;
    }
   /* public void ToggleNoWeapon(bool toggle)
    {
        Debug.Log($"ToggleNoWeapon called with toggle: {toggle}");
        Debug.Log($"Current layer weights before toggle:");
        Debug.Log($"{anime.GetLayerName(oneHandedLayerIndex)} : {anime.GetLayerWeight(oneHandedLayerIndex)}");
        Debug.Log($"{anime.GetLayerName(twoHandedLayerIndex)} : {anime.GetLayerWeight(twoHandedLayerIndex)}");
        Debug.Log($"{anime.GetLayerName(noWeaponLayerIndex)} : {anime.GetLayerWeight(noWeaponLayerIndex)}");


        if (toggle)
        {
            Debug.Log("Setting No Weapon Animations");
            anime.SetBool("NoWeapon", true);
            oneHandedRig.weight = 0;
            anime.SetLayerWeight(oneHandedLayerIndex, 0);

            twoHandedRig.weight = 0;
            anime.SetLayerWeight(twoHandedLayerIndex, 0);
            twoHanded = false;
            aimTarget.SetActive(false);
            tbIk.weight = 0;

            anime.SetLayerWeight(noWeaponLayerIndex, 1);
        }
        else
        {
            anime.SetBool("NoWeapon", false);

            oneHandedRig.weight = 1;
            anime.SetLayerWeight(oneHandedLayerIndex, 1);

            twoHandedRig.weight = 0;
            anime.SetLayerWeight(twoHandedLayerIndex, 0);
            twoHanded = false;
            aimTarget.SetActive(true);

            tbIk.weight = 1;

            anime.SetLayerWeight(noWeaponLayerIndex, 0);
        }
        Debug.Log($"layer weights After toggle:");

        Debug.Log($"{anime.GetLayerName(oneHandedLayerIndex)} : {anime.GetLayerWeight(oneHandedLayerIndex)}");
        Debug.Log($"{anime.GetLayerName(twoHandedLayerIndex)} : {anime.GetLayerWeight(twoHandedLayerIndex)}");
        Debug.Log($"{anime.GetLayerName(noWeaponLayerIndex)} : {anime.GetLayerWeight(noWeaponLayerIndex)}");
    }*/
     public void ActivateATrigger(string triggerName)
    {
        anime.SetTrigger(triggerName);
    }
    public void ToggleRespawn(bool toggle)
    {
        anime.SetBool("Respawn", toggle);
    }
    public void ToggleUnscaledUpdateMode(bool toggle)
    {
        if(toggle)
            anime.updateMode = AnimatorUpdateMode.UnscaledTime;
        else
            anime.updateMode = AnimatorUpdateMode.Normal;

    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
