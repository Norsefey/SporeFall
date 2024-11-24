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

    [Header("Left Hand")]
    [SerializeField] private TwoBoneIKConstraint tbIk; // add or remove weight to constraint to enable or disable it
    [SerializeField] private Transform leftHandTarget;// the IK target, will move to weapon hold slot


    [Header("Rigs")]// IK rigs for Aiming
    [SerializeField] private Rig oneHandedRig;
    [SerializeField] private Rig twoHandedRig;
    // For animation layers
    private bool twoHanded = false;
    private int oneHandedLayerIndex;// will holds one handed animations
    private int twoHandedLayerIndex;// holds two handed animations
    private int noWeaponLayerIndex;// for when player holds no weapons
    // Animation states
    private bool isWalking = false;
    private bool isAiming = false;
    private void Start()
    {
        // get the index numbers for the animation layers
        oneHandedLayerIndex = anime.GetLayerIndex("OneHanded");
        twoHandedLayerIndex = anime.GetLayerIndex("TwoHanded");
        noWeaponLayerIndex = anime.GetLayerIndex("NoWeapon");
    }


    // Update is called once per frame
    void Update()
    {
        // keep weapon to hand position
        WeaponPosition();
        WalkCheck();
    }
    private void WeaponPosition()
    {
        if(pMan.currentWeapon == null)
            return;
        weaponSlot.position = rightHandBone.position;

        Vector3 handForward = rightHandBone.up;
        weaponSlot.rotation = Quaternion.LookRotation(handForward);

        // Move left hand onto support position
        if(pMan.currentWeapon != null && ( isAiming || twoHanded))
            leftHandTarget.position = pMan.currentWeapon.secondHandHold.position;

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
    public void ToggleAimAnime(bool toggle)
    {
        if(pMan.currentWeapon == null)
            return;

        if (toggle)
        {
            tbIk.weight = 1;
        }
        else if(!twoHanded)
        {
            tbIk.weight = 0;
        }
        anime.SetBool("IsAiming", toggle);
        isAiming = toggle;
    }
    public void ToggleTwoHanded(bool toggle)
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
       
    }
    public void ToggleIKAim(bool toggle)
    {
        if(pMan.currentWeapon == null)
            return;
        if (!toggle)
        {
            if (twoHanded)
            {
                twoHandedRig.weight = 0;
            }
            else
            {
                oneHandedRig.weight = 0;
            }
        }
        else
        {
            if (twoHanded)
            {
                twoHandedRig.weight = 1;
            }
            else
            {
                oneHandedRig.weight = 1;
            }
        }
    
    }
    public void ToggleNoWeapon(bool toggle)
    {
        if (toggle)
        {
            oneHandedRig.weight = 0;
            anime.SetLayerWeight(oneHandedLayerIndex, 0);

            twoHandedRig.weight = 0;
            anime.SetLayerWeight(twoHandedLayerIndex, 0);
            twoHanded = false;

            tbIk.weight = 0;

            anime.SetLayerWeight(noWeaponLayerIndex, 1);
        }
        else
        {
            oneHandedRig.weight = 1;
            anime.SetLayerWeight(oneHandedLayerIndex, 1);

            twoHandedRig.weight = 0;
            anime.SetLayerWeight(twoHandedLayerIndex, 0);
            twoHanded = false;

            tbIk.weight = 1;

            anime.SetLayerWeight(noWeaponLayerIndex, 0);
        }
       
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
