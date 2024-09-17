using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public PlayerMovement pController;
    [SerializeField] public TPSCamera pCamera;
    [SerializeField] public GameObject pVisual;
    public GameObject currentWeapon;
    // Input Maps
    private InputActionAsset inputAsset;
    private InputActionMap player;
    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction jumpAction;
    public InputAction aimAction;
    public InputAction fireAction;
    public InputAction camRotateAction;
    public InputAction camZoomAction;

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
    }

    private void OnEnable()
    {
        // put in actions like attack or jump here to enabled them
        moveAction = player.FindAction("Move");
        camRotateAction = player.FindAction("Look");
        jumpAction = player.FindAction("Jump");
        aimAction = player.FindAction("Aim");
        fireAction = player.FindAction("Fire");
        // this assigns calls to the actions
        // When Action Starts
        jumpAction.started += pController.JumpCall;
        aimAction.started += pCamera.AimSightCall;
        fireAction.performed += Fire;
        // When Action Stops
        aimAction.canceled += pCamera.DefaultSightCall;
        // this enables the controls
        player.Enable();
    }
    private void OnDisable()
    {
        // remove calls
        fireAction.started -= Fire;
        jumpAction.started -= pController.JumpCall;
        aimAction.started -= pCamera.AimSightCall;
        aimAction.canceled -= pCamera.DefaultSightCall;
        player.Disable();
    }

    void Fire(InputAction.CallbackContext obj)
    {
        // Send Message to current weapon to fire
        Debug.Log("Fire");
    }
}
