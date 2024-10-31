using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpWeapon : Interactables
{
    private string promptText;
    [Space(5), Header("Pick Up")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private float rotSpeed = 45;
    private void Start()
    {
        promptText = "Pick Up: " + weapon.weaponName;
    }
    private void LateUpdate()
    {
        weapon.transform.Rotate(new Vector3(0, rotSpeed * Time.deltaTime, 0));
    }
    public override void ItemAction()
    {
        player.nearByWeapon = weapon.gameObject;
        player.pUI.EnablePrompt(promptText);
    }
    public override void Interact(InputAction.CallbackContext context)
    {
        player.PickUpWeapon();
        DestroyIntractable();
    }
    public override void RemoveAction()
    {
        player.nearByWeapon = null;
        player.pUI.DisablePrompt();
    }
}
