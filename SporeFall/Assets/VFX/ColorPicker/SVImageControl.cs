using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SVImageControl : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField] private Image pickerImage;
    [SerializeField] private ColorPickerControl colorPickerControl;
    
    private RawImage SVImage;
    private RectTransform rectTransform, pickerTransform;

    // Current normalized position values (0-1 range)
    private float currentXNorm = 0;
    private float currentYNorm = 0;

    private void Start()
    {
        SVImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        pickerTransform = pickerImage.GetComponent<RectTransform>();
        pickerTransform.localPosition = new Vector2(-(rectTransform.sizeDelta.x * 0.5f), -(rectTransform.sizeDelta.y * 0.5f));
        
    }
    // Called by ColorPickerControl for gamepad input
    public void AdjustValueWithGamepad(float xDelta, float yDelta)
    {
        // Update normalized position based on gamepad input
        currentXNorm = Mathf.Clamp01(currentXNorm + xDelta);
        currentYNorm = Mathf.Clamp01(currentYNorm + yDelta);

        // Convert normalized position to local space
        float deltaX = rectTransform.sizeDelta.x * 0.5f;
        float deltaY = rectTransform.sizeDelta.y * 0.5f;

        float posX = (currentXNorm * rectTransform.sizeDelta.x) - deltaX;
        float posY = (currentYNorm * rectTransform.sizeDelta.y) - deltaY;

        // Update picker position
        pickerTransform.localPosition = new Vector3(posX, posY, 0);

        // Update picker color for visual feedback
        pickerImage.color = Color.HSVToRGB(0, 0, 1 - currentYNorm);

        Debug.Log(currentXNorm + " " + currentYNorm);

        // Update the color in the main control
        colorPickerControl.SetSV(currentXNorm, currentYNorm);
    }
    void UpdateColor(PointerEventData eventData)
    {
        // get current position of pointer
        Vector3 pos = rectTransform.InverseTransformPoint(eventData.position);

        float deltaX = rectTransform.sizeDelta.x * 0.5f;
        float deltaY = rectTransform.sizeDelta.y * 0.5f;

        // Prevent from leaving image space
        if (pos.x < -deltaX)
        {
            pos.x = -deltaX;
        }
        else if (pos.x > deltaX)
        {
            pos.x = deltaX;
        }

        if (pos.y < -deltaY)
        {
            pos.y = -deltaY;
        }
        else if (pos.y > deltaY)
        {
            pos.y = deltaY;
        }

        float x = pos.x + deltaX;
        float y = pos.y + deltaY;

        currentXNorm = x / rectTransform.sizeDelta.x;
        currentYNorm = y / rectTransform.sizeDelta.y;

        pickerTransform.localPosition = pos;
        pickerImage.color = Color.HSVToRGB(0, 0, 1 - currentYNorm);

        colorPickerControl.SetSV(currentXNorm, currentYNorm);
    }

    // Mouse Control events
    public void OnDrag(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }
}
