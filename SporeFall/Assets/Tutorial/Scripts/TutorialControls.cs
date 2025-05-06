using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TutorialControls : MonoBehaviour
{
    public static TutorialControls Instance;

    //Shows different controls based on what device is being used
    public string continueInput;
    public string moveInput;
    public string sprintInput;
    public string jumpInput;
    public string lookInput;
    public string aimInput;
    public string shootInput;
    public string reloadInput;
    public string pickupInput;
    public string dropInput;
    public string buildInput;
    public string editInput;
    public string scrollInput;
    public string destroyInput;
    public string pauseInput;
    public string skipInput;

    public string continueInput2;
    public string moveInput2;
    public string sprintInput2;
    public string jumpInput2;
    public string lookInput2;
    public string aimInput2;
    public string shootInput2;
    public string reloadInput2;
    public string pickupInput2;
    public string dropInput2;
    public string buildInput2;
    public string editInput2;
    public string scrollInput2;
    public string destroyInput2;
    public string pauseInput2;
    public string skipInput2;

    public bool usingKeyboard = false;
    public bool usingXbox = false;
    public bool usingPlaystation = false;
    public bool controlsSet = false;
    public bool playerActive = false;
    public bool gamepadActive = false;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for gamepad input
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        if (playerActive == true && controlsSet == false)
        {
            if (usingKeyboard == true)
            {
                Debug.Log("Starting keyboard tutorial");
                //SetKeyboardInputs();
                controlsSet = true;
            }

            else if (usingXbox == true)
            {
                Debug.Log("Starting xbox tutorial");
                //SetXboxInputs();
                controlsSet = true;
            }
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    #region set Inputs
    public void SetKeyboardInputsP1()
    {
        continueInput = "C";
        moveInput = "WASD";
        sprintInput = "Shift";
        jumpInput = "Space";
        lookInput = "Mouse";
        aimInput = "Right click";
        shootInput = "Left click";
        reloadInput = "R";
        pickupInput = "F";
        dropInput = "Q";
        buildInput = "B";
        editInput = "F";
        scrollInput = "mousewheel or number keys";
        destroyInput = "X";
        pauseInput = "Esc";
        skipInput = "Z";
    }

    public void SetKeyboardInputsP2()
    {
        aimInput2 = "Right click";
        shootInput2 = "Left click";
        editInput2 = "F";
        scrollInput2 = "mousewheel or number keys";
        destroyInput2 = "X";
    }

    public void SetXboxInputsP1()
    {
        gamepadActive = true;
        continueInput = "A";
        moveInput = "Left stick";
        sprintInput = "Hold left stick";
        jumpInput = "A";
        lookInput = "Right stick";
        aimInput = "Left trigger";
        shootInput = "Right trigger";
        reloadInput = "Y";
        pickupInput = "X";
        dropInput = "Y";
        buildInput = "B";
        editInput = "Y";
        scrollInput = "left and right bumpers";
        destroyInput = "B";
        pauseInput = "Options";
        skipInput = "down on the D-Pad";
    }

    public void SetXboxInputsP2()
    {
        gamepadActive = true;
        aimInput2 = "Left trigger";
        shootInput2 = "Right trigger";
        editInput2 = "Y";
        scrollInput2 = "left and right bumpers";
        destroyInput2 = "B";
    }

    #endregion

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            gamepadActive = true;
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            gamepadActive = false;
        }
    }
}
