using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public string scrollInput;
    public string destroyInput;
    public string pauseInput;
    public string skipInput;

    public bool usingKeyboard = false;
    public bool usingXbox = false;
    public bool usingPlaystation = false;
    public bool controlsSet = false;
    public bool playerActive = false;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerActive == true && controlsSet == false)
        {
            if (usingKeyboard == true)
            {
                Debug.Log("Starting keyboard tutorial");
                SetKeyboardInputs();
                controlsSet = true;
            }

            else if (usingXbox == true)
            {
                Debug.Log("Starting xbox tutorial");
                SetXboxInputs();
                controlsSet = true;
            }

            else if (usingPlaystation == true)
            {
                Debug.Log("Starting PS tutorial");
                SetPlaystationInputs();
                controlsSet = true;
            }
        }
    }

    #region set Inputs
    private void SetKeyboardInputs()
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
        scrollInput = "mousewheel";
        destroyInput = "X";
        pauseInput = "Esc";
        skipInput = "Z";
    }

    private void SetXboxInputs()
    {
        continueInput = "A";
        moveInput = "Left stick";
        sprintInput = "Hold left stick";
        jumpInput = "A";
        lookInput = "Right stick";
        aimInput = "Left trigger";
        shootInput = "Right trigger";
        reloadInput = "B";
        pickupInput = "X";
        dropInput = "B";
        buildInput = "Y";
        scrollInput = "left and right bumpers";
        destroyInput = "B";
        pauseInput = "Options";
        skipInput = "down on the D-Pad";
    }

    private void SetPlaystationInputs()
    {
        continueInput = "X";
        moveInput = "Left stick";
        sprintInput = "Hold left stick";
        jumpInput = "X";
        lookInput = "Right stick";
        aimInput = "Left trigger";
        shootInput = "Right trigger";
        reloadInput = "Circle";
        pickupInput = "Square";
        dropInput = "Circle";
        buildInput = "Triangle";
        scrollInput = "left and right bumpers";
        destroyInput = "Circle";
        pauseInput = "Options";
        skipInput = "down on the D-Pad";
    }
    #endregion
}
