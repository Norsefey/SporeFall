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

    [SerializeField] private Rig bodyLook;
    // Animation states
    private bool isWalking = false;
    private bool isAiming = false;
    private void Start()
    {

        if (pMan.currentWeapon == null)
        {
            Debug.Log("No Weapon");
            SetWeaponHoldAnimation(0);
            pMan.pUI.ToggleWeaponUI(false);
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
    }
    public void ToggleIKAim(bool toggle)
    {
        bodyLook.weight = toggle ? 1 : 0;
    }
     public void ActivateATrigger(string triggerName)
    {
        anime.ResetTrigger(triggerName);
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
