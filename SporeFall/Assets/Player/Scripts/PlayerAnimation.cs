using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private PlayerManager pMan;

    [SerializeField] private Animator anime;
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private Transform rightHandBone;

    private bool isWalking = false;
    private bool isAiming = false;


    private bool twoHanded = false;
    private int oneHandedLayerIndex;
    private int twoHandedLayerIndex;

    private void Start()
    {
        oneHandedLayerIndex = anime.GetLayerIndex("OneHanded");
        twoHandedLayerIndex = anime.GetLayerIndex("TwoHanded");
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleTwoHanded(!twoHanded);
        }

        if (Input.GetKeyDown(KeyCode.M))
            Time.timeScale = .5f;
        else if (Input.GetKeyUp(KeyCode.M))
            Time.timeScale = 1;

        weaponSlot.position = rightHandBone.position;
        Vector3 handForward = rightHandBone.up;
        weaponSlot.rotation = Quaternion.LookRotation(handForward);

        if(!isWalking && pMan.pInput.moveAction.ReadValue<Vector2>() != Vector2.zero)
        {
            isWalking = true;
            ToggleWalkAnime(true);
        }
        else if(isWalking && pMan.pInput.moveAction.ReadValue<Vector2>() == Vector2.zero)
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
        anime.SetBool("IsAiming", toggle);
    }
    public void ToggleTwoHanded(bool toggle)
    {
        if (toggle)
        {
            anime.SetLayerWeight(oneHandedLayerIndex, 0);
            anime.SetLayerWeight(twoHandedLayerIndex, 1);
            twoHanded = true;
        }
        else
        {
            anime.SetLayerWeight(oneHandedLayerIndex, 0);
            anime.SetLayerWeight(twoHandedLayerIndex, 1);
            twoHanded = false;
        }
       
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
