using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpWeapon : Interactables
{
    private string promptText;
    [Space(5), Header("Pick Up")]
    [SerializeField] private Weapon pickUp;
    [SerializeField] private float rotSpeed = 45;
    private void Start()
    {
        promptText = "Pick Up: " + pickUp.weaponName;
    }
    private void LateUpdate()
    {
        pickUp.transform.Rotate(new Vector3(0, rotSpeed * Time.deltaTime, 0));
    }
    public override void ItemAction()
    {
        player.nearByPickUp = pickUp.gameObject;
        player.pUI.EnablePrompt(promptText);
    }
    public override void Interact(InputAction.CallbackContext context)
    {
        player.PickUpWeapon();
    }
    public override void RemoveAction()
    {
        player.nearByPickUp = null;
        player.pUI.DisablePrompt();
    }
}
