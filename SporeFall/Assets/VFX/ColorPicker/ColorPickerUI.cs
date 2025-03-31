using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ColorPickerUI : MonoBehaviour
{
    //Script that controls UI, mostly navigation for controller, for the Color Picker scene
    
    public GameObject firstButton;
    public GameObject finishButton;

    [HideInInspector] public Navigation firstButtonNav = new Navigation();
    [HideInInspector] public Navigation finishButtonNav = new Navigation();

    public GameObject currentSVButton;
    public GameObject currentHueButton;

    //Same red as text on most menus
    [HideInInspector] public Color shroomRed = new Color(1f, 0.3254902f, 0.3215686f, 1f);

    public bool canSelectColor = false;

    [Header("Player 1 Variables")]
    // to activate the gamepad controls on the selected color picker
    [SerializeField] private ColorPickerControl p1PC;
    [SerializeField] private ColorPickerControl p1SC;

    public GameObject colorPickerButton1p1;
    [SerializeField] GameObject svButton1p1;
    [SerializeField] GameObject svSlider1p1;
    [SerializeField] GameObject hueButton1p1;
    [SerializeField] GameObject hueSlider1p1;

    public GameObject colorPickerButton2p1;
    [SerializeField] GameObject svButton2p1;
    [SerializeField] GameObject svSlider2p1;
    [SerializeField] GameObject hueButton2p1;
    [SerializeField] GameObject hueSlider2p1;

    [Header("Player 2 Variables")]
    [SerializeField] private ColorPickerControl p2PC;
    [SerializeField] private ColorPickerControl p2SC;

    public GameObject colorPickerButton1p2;
    [SerializeField] GameObject svButton1p2;
    [SerializeField] GameObject svSlider1p2;
    [SerializeField] GameObject hueButton1p2;
    [SerializeField] GameObject hueSlider1p2;

    public GameObject colorPickerButton2p2;
    [SerializeField] GameObject svButton2p2;
    [SerializeField] GameObject svSlider2p2;
    [SerializeField] GameObject hueButton2p2;
    [SerializeField] GameObject hueSlider2p2;

    private bool isControllerConnected = false;

    private void Awake()
    {
        firstButtonNav.mode = Navigation.Mode.Explicit;
        finishButtonNav.mode = Navigation.Mode.Explicit;

        firstButtonNav.selectOnRight = colorPickerButton1p1.GetComponent<Button>();
        firstButtonNav.selectOnDown = finishButton.GetComponent<Button>();
        firstButton.GetComponent<Button>().navigation = firstButtonNav;

        finishButtonNav.selectOnLeft = firstButton.GetComponent<Button>();
        finishButtonNav.selectOnUp = colorPickerButton2p1.GetComponent<Button>();
        finishButton.GetComponent<Button>().navigation = finishButtonNav;

        //Doesn't matter which ones are selected first
        currentSVButton = svButton1p1;
        currentHueButton = hueButton1p1;
    }

    private void Start()
    {
        Debug.Log("There are " + InputSystem.devices.Count + "devices connected");
        InputSystem.onDeviceChange += OnDeviceChange;
        if (InputSystem.devices.Count > 2)
        {
            //If a controller is being used, highlights the first button
            //If a controller is not being used, no buttons are highlighted
            isControllerConnected = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    #region Player One
    #region Primary Color Picker
    public void SelectColorPicker1P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svButton1p1);
        currentSVButton = svButton1p1;
        currentHueButton = hueButton1p1;
        canSelectColor = false;
    }
    public void SelectSVSlider1P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svSlider1p1);
        EventSystem.current.sendNavigationEvents = false;
        // activates SV Controls and Deactivates Hue controls just in case
        p1PC.isSVSelected = true;
        p1PC.selected = true;
        p1PC.isHueSelected = false;
        //Changes color of selected button to show what you're changing the color on
        svButton1p1.GetComponent<Image>().color = shroomRed;
        StartCoroutine(SelectionColdown());
    }
    public void SelectHueSlider1P1()
    {
        // Disabled to prevent Slider jumping and stuttering
        //EventSystem.current.SetSelectedGameObject(null);
        //EventSystem.current.SetSelectedGameObject(hueSlider1p1);

        // activates Hue controls, no need to deactivate SV control as this is handled in color picker
        EventSystem.current.sendNavigationEvents = false;
        p1PC.isHueSelected = true;
        hueButton1p1.GetComponent<Image>().color = shroomRed;
        StartCoroutine(SelectionColdown());
    }
    public void DoneColorPicker1P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(colorPickerButton1p1);
        EventSystem.current.sendNavigationEvents = true;
        p1PC.isSVSelected = false;
        p1PC.selected = false;
        p1PC.isHueSelected = false;
    }
    #endregion

    #region Secondary Color Picker
    public void SelectColorPicker2P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svButton2p1);
        currentSVButton = svButton2p1;
        currentHueButton = hueButton2p1;
        canSelectColor = false;
    }
    public void SelectSVSlider2P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svSlider1p1);
        EventSystem.current.sendNavigationEvents = false;
        // activates SV Controls and Deactivates Hue controls just in case
        p1SC.isSVSelected = true;
        p1SC.selected = true;
        p1SC.isHueSelected = false;
        //Changes color of selected button to show what you're changing the color on
        svButton2p1.GetComponent<Image>().color = shroomRed;
        StartCoroutine(SelectionColdown());
    }
    public void SelectHueSlider2P1()
    {
        EventSystem.current.sendNavigationEvents = false;
        p1SC.isHueSelected = true;
        hueButton2p1.GetComponent<Image>().color = shroomRed;
        StartCoroutine(SelectionColdown());
    }
    public void DoneColorPicker2P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(colorPickerButton2p1);
        p1SC.isSVSelected = false;
        p1SC.selected = false;
        p1SC.isHueSelected = false;
    }
    #endregion
    #endregion
    #region Player Two
    #region Primary Color Picker
    public void SelectColorPicker1P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svButton1p2);
        currentSVButton = svButton1p2;
        currentHueButton = hueButton1p2;
        canSelectColor = false;
    }
    public void SelectSVSlider1P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svSlider1p2);
        // So that nothing else can be selected
        EventSystem.current.sendNavigationEvents = false;
        // activates SV Controls and Deactivates Hue controls just in case
        p2PC.isSVSelected = true;
        p2PC.selected = true;
        p2PC.isHueSelected = false;
        //Changes color of selected button to show what you're changing the color on
        svButton1p2.GetComponent<Image>().color = shroomRed;
        StartCoroutine(SelectionColdown());
    }
    public void SelectHueSlider1P2()
    {
        EventSystem.current.sendNavigationEvents = false;
        p2PC.isHueSelected = true;
        hueButton1p2.GetComponent<Image>().color = shroomRed;
        StartCoroutine(SelectionColdown());
    }
    public void DoneColorPicker1P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(colorPickerButton1p2);
        EventSystem.current.sendNavigationEvents = true;
        p2PC.isSVSelected = false;
        p2PC.selected = false;
        p2PC.isHueSelected = false;
    }
    #endregion

    #region Secondary Color Picker
    public void SelectColorPicker2P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svButton2p2);
        currentSVButton = svButton2p2;
        currentHueButton = hueButton2p2;
        canSelectColor = false;
    }

    public void SelectSVSlider2P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svSlider2p2);
        // So that nothing else can be selected
        EventSystem.current.sendNavigationEvents = false;
        // activates SV Controls and Deactivates Hue controls just in case
        p2SC.isSVSelected = true;
        p2SC.selected = true;
        p2SC.isHueSelected = false;
        //Changes color of selected button to show what you're changing the color on
        svButton2p2.GetComponent<Image>().color = shroomRed;
        StartCoroutine(SelectionColdown());
    }

    public void SelectHueSlider2P2()
    {
        EventSystem.current.sendNavigationEvents = false;
        p2SC.isHueSelected = true;
        hueButton2p2.GetComponent<Image>().color = shroomRed;
        StartCoroutine(SelectionColdown());
    }

    public void DoneColorPicker2P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(colorPickerButton2p2);
        EventSystem.current.sendNavigationEvents = true;
        p2SC.isSVSelected = false;
        p2SC.selected = false;
        p2SC.isHueSelected = false;
    }
    #endregion
    #endregion

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log($"Device Connected: {device.displayName}");
            isControllerConnected = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            Debug.Log($"Device Disconnected: {device.displayName}");
            isControllerConnected = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }


    IEnumerator SelectionColdown()
    {
        yield return new WaitForSeconds(.1f);
        canSelectColor = true;
        Debug.Log("Can select color");
    }

}
