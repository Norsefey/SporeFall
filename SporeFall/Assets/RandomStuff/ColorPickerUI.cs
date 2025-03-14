using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPickerUI : MonoBehaviour
{
    public GameObject firstButton;
    public GameObject finishButton;

    [HideInInspector] public Navigation firstButtonNav = new Navigation();
    [HideInInspector] public Navigation finishButtonNav = new Navigation();

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

    private void Awake()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton);

        firstButtonNav.mode = Navigation.Mode.Explicit;
        finishButtonNav.mode = Navigation.Mode.Explicit;

        firstButtonNav.selectOnRight = colorPickerButton1p1.GetComponent<Button>();
        firstButtonNav.selectOnDown = finishButton.GetComponent<Button>();
        firstButton.GetComponent<Button>().navigation = firstButtonNav;

        finishButtonNav.selectOnLeft = firstButton.GetComponent<Button>();
        finishButtonNav.selectOnUp = colorPickerButton2p1.GetComponent<Button>();
        finishButton.GetComponent<Button>().navigation = finishButtonNav;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Player One
    #region Primary Color Picker
    public void SelectColorPicker1P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svButton1p1);
        p1PC.selected = true;
    }

    public void SelectSVSlider1P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svSlider1p1);
        p1PC.isHueSelected = false;
    }

    public void SelectHueSlider1P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(hueSlider1p1);
        p1PC.isHueSelected = true;
    }

    public void DoneColorPicker1P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(colorPickerButton1p1);
    }
    #endregion

    #region Secondary Color Picker
    public void SelectColorPicker2P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svButton2p1);
    }

    public void SelectSVSlider2P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svSlider2p1);
    }

    public void SelectHueSlider2P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(hueSlider2p1);
    }

    public void DoneColorPicker2P1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(colorPickerButton2p1);
    }
    #endregion
    #endregion
    #region Player Two
    #region Primary Color Picker
    public void SelectColorPicker1P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svButton1p2);
    }

    public void SelectSVSlider1P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svSlider1p2);
    }

    public void SelectHueSlider1P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(hueSlider1p2);
    }

    public void DoneColorPicker1P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(colorPickerButton1p2);
    }
    #endregion

    #region Secondary Color Picker
    public void SelectColorPicker2P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svButton2p2);
    }

    public void SelectSVSlider2P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(svSlider2p2);
    }

    public void SelectHueSlider2P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(hueSlider2p2);
    }

    public void DoneColorPicker2P2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(colorPickerButton2p2);
    }
    #endregion
    #endregion
}
