using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimation : MonoBehaviour
{
    private PlayerManager pMan;

    [SerializeField] private Animator anime;
    [SerializeField] private Transform weaponSlot; // to always have gun in right hand
    [SerializeField] private Transform rightHandBone;// where the weapon will move to

    [Header("Left Hand")]
    [SerializeField] private TwoBoneIKConstraint tbIk; // add or remove weight to constraint to enable or disable it
    [SerializeField] private Transform leftHandTarget;// the IK target, will move to weapon hold slot


    [Header("Rigs")]// IK rigs for Aiming
    [SerializeField] private Rig oneHandedRig;
    [SerializeField] private Rig twoHandedRig;
    // For animation layers
    private bool twoHanded = false;
    private int oneHandedLayerIndex; // will holds one handed animations
    private int twoHandedLayerIndex;// holds two handed animations
    // Animation states
    private bool isWalking = false;
    private bool isAiming = false;
    private void Start()
    {
        // get the index numbers for the animation layers
        oneHandedLayerIndex = anime.GetLayerIndex("OneHanded");
        twoHandedLayerIndex = anime.GetLayerIndex("TwoHanded");
    }


    // Update is called once per frame
    void Update()
    {
        // temp code to adjust animation speeds
        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = .1f;
        }
        // return time scale to normal
        if (Input.GetKeyDown(KeyCode.M))
            Time.timeScale = 1;
        // keep weapon to hand position
        WeaponPosition();
        // check if we are walking or idle, will add sprint later
        WalkCheck();
    }
    private void WeaponPosition()
    {
        weaponSlot.position = rightHandBone.position;

       /* if (!twoHanded)
        {*/
        Vector3 handForward = rightHandBone.up;
        weaponSlot.rotation = Quaternion.LookRotation(handForward);

        
        if(isAiming || twoHanded)
            leftHandTarget.position = pMan.currentWeapon.secondHandHold.position;

    }
    private void WalkCheck()
    {
        if (!isWalking && pMan.pInput.moveAction.ReadValue<Vector2>() != Vector2.zero)
        {
            isWalking = true;
            ToggleWalkAnime(true);
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
    public void ToggleAimAnime(bool toggle)
    {
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
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
