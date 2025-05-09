using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public enum ColorPickerMode
{
    Player1Primary,
    Player1Secondary,
    Player2Primary,
    Player2Secondary
}
public class ColorPickerControl : MonoBehaviour
{
    [SerializeField] private ColorPickerUI ColorPickerUI;
    
    public float currentHue, currentSat, currentVal;

    [SerializeField]
    private RawImage hueImage, satValImage, outputImage;
    [SerializeField] private Slider hueSlider;
    [SerializeField] private TMP_InputField hexInputField;

    [SerializeField]
    Texture2D hueTexture, svTexture, outputTexture;

    [SerializeField]
    Material materialToChange;
    
    [Header("Color Picker Configuration")]
    [SerializeField] private ColorPickerMode currentMode = ColorPickerMode.Player1Primary;

    [Header("Gamepad Settings")]
    [SerializeField] private float gamepadSensitivity = 0.5f;
    [SerializeField] private SVImageControl svImageControl;
    public bool isHueSelected = false;
    public bool isSVSelected = false;
    public bool selected = false;

    private void Awake()
    {

        CreateHueImage();

        CreateSVImage();

        CreateOutputImage();
        
        SetInitialColor();
    }
    private void Update()
    {
        if(selected)
            HandleGamepadInput();
    }
    private void HandleGamepadInput()
    {
        // Check for gamepad input
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        // Switch between hue slider and SV picker with shoulder buttons
        if (gamepad.leftShoulder.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame)
        {
            isHueSelected = !isHueSelected;
            isSVSelected = !isSVSelected;
            // We can put the highlight functionality here??
            Debug.Log(isHueSelected ? "Hue slider selected" : "SV picker selected");
            //Changes color to show what is currently selected when the bumpers are hit
            if (isHueSelected)
            {
                ColorPickerUI.currentHueButton.GetComponent<Image>().color = ColorPickerUI.shroomRed;
                ColorPickerUI.currentSVButton.GetComponent<Image>().color = Color.white;
            }
            else
            {
                ColorPickerUI.currentSVButton.GetComponent<Image>().color = ColorPickerUI.shroomRed;
                ColorPickerUI.currentHueButton.GetComponent<Image>().color = Color.white;
            }
        }

        // Vertical movement controls the selected component
        float verticalInput = gamepad.leftStick.y.ReadValue();

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            if (isHueSelected && ColorPickerUI.canSelectColor)
            {
                // Control hue slider
                hueSlider.value = Mathf.Clamp01(hueSlider.value + verticalInput * Time.deltaTime * gamepadSensitivity);
                UpdateSVImage();
            }
            else if (isSVSelected && ColorPickerUI.canSelectColor)
            {
                // Delegate SV control to SVImageControl
                if (svImageControl != null)
                {
                    svImageControl.AdjustValueWithGamepad(0, verticalInput * Time.deltaTime * gamepadSensitivity);
                }
            }
        }

        // Horizontal movement for SV picker when it's selected
        float horizontalInput = gamepad.leftStick.x.ReadValue();

        if (!isHueSelected && Mathf.Abs(horizontalInput) > 0.1f && svImageControl != null && ColorPickerUI.canSelectColor)
        {
            svImageControl.AdjustValueWithGamepad(horizontalInput * Time.deltaTime * gamepadSensitivity, 0);
        }

        // Button to confirm/exit color picker
        if (gamepad.buttonSouth.wasPressedThisFrame && ColorPickerUI.canSelectColor) 
        {
            if (MenuSounds.Instance != null)
            {
                MenuSounds.Instance.PlayPressedSound();
            }
            // Confirm color selection
            Debug.Log("Color confirmed: " + Color.HSVToRGB(currentHue, currentSat, currentVal));
            // Then do stuff to exit the selection
            if (isHueSelected)
            {
                Debug.Log("Exiting to hue button");
                isHueSelected = false;
                isSVSelected = false;
                ColorPickerUI.canSelectColor = false;
                EventSystem.current.sendNavigationEvents = true;
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(ColorPickerUI.currentHueButton);
                Debug.Log("Cannot select color");
                ColorPickerUI.currentHueButton.GetComponent<Image>().color = Color.white;
            }
            else if (isSVSelected)
            {
                Debug.Log("Exiting to SV button");
                isSVSelected = false;
                isHueSelected = false;
                ColorPickerUI.canSelectColor = false;
                EventSystem.current.sendNavigationEvents = true;
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(ColorPickerUI.currentSVButton);
                Debug.Log("Cannot select color");
                ColorPickerUI.currentSVButton.GetComponent<Image>().color = Color.white;
            }

        }
    }
    private void CreateHueImage()
    {
        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.name = "HueTexture";

        for (int i = 0; i < hueTexture.height; i++)
        {
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1f));
        }

        hueTexture.Apply();
        currentHue = 0;

        hueImage.texture = hueTexture;
    }
    private void CreateSVImage()
    {
        svTexture = new Texture2D (16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SatValTexture";

        for(int y = 0;y < svTexture.height; y++)
        {
            for(int x = 0;x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue,
                                                            (float)x / svTexture.width,
                                                                (float)y / svTexture.height));
            }
        }

        svTexture.Apply();
        currentSat = 0;
        currentVal = 0;

        satValImage.texture = svTexture;
    }
    private void CreateOutputImage()
    {
        outputTexture = new Texture2D (1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputTexture.name = "OutputTexture";

        Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColor);
        }

        outputTexture.Apply();

        outputImage.texture = outputTexture;
    }
    private void SetInitialColor()
    {

        if (PersistentGameManager.Instance != null)
        {
            Color currentColor = PersistentGameManager.Instance.GetColor(currentMode);
            materialToChange.SetColor("_BaseColor", currentColor);
            Color.RGBToHSV(currentColor, out currentHue, out currentSat, out currentVal);
            Debug.Log(currentColor.ToString());
        }
        // had to change to an instance based color setter and getter in order for colors to save between scenes
        /*    Color currentColor = materialToChange.GetColor("_BaseColor");
              Debug.Log(currentColor.ToString());
              Color.RGBToHSV(currentColor, out currentHue, out currentSat, out currentVal);*/

        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue,
                                                            (float)x / svTexture.width,
                                                                (float)y / svTexture.height));
            }
        }
        svTexture.Apply();

        hueSlider.value = currentHue;

    }
    private void UpdateOutputImage()
    {
        Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColor);
        }

        outputTexture.Apply();

        materialToChange.SetColor("_BaseColor", currentColor);

        // Store the selected color in ColorManager
        if (PersistentGameManager.Instance != null)
        {
            PersistentGameManager.Instance.SetColor(currentMode, currentColor);
        }
    }
    public void SetSV(float s, float v)
    {
        currentSat = s;
        currentVal = v;

        UpdateOutputImage();
    }
    // called by slider
    public void UpdateSVImage()
    {
        currentHue = hueSlider.value;

        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, 
                                                            (float)x / svTexture.width,
                                                                (float)y / svTexture.height));
            }
        }

        svTexture.Apply();
        UpdateOutputImage();
    }
}
