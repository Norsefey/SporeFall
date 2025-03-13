using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerControl : MonoBehaviour
{
    public float currentHue, currentSat, currentVal;

    [SerializeField]
    private RawImage hueImage, satValImage, outputImage;
    [SerializeField] private Slider hueSlider;
    [SerializeField] private TMP_InputField hexInputField;

    [SerializeField]
    Texture2D hueTexture, svTexture, outputTexture;

    [SerializeField]
    Material materialToChange;

    private void Awake()
    {
        CreateHueImage();

        CreateSVImage();

        CreateOutputImage();

        SetInitialColor();
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
        Color currentColor = materialToChange.GetColor("_BaseColor");
        Debug.Log(currentColor.ToString());
        
        Color.RGBToHSV(currentColor, out currentHue, out currentSat, out currentVal);

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
