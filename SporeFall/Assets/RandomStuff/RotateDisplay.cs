using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RotateDisplay : MonoBehaviour
{
    [SerializeField] private ColorPickerUI colorPickerUI;
    private bool canRotate = false;
    public float rotationSpeed = 5f; // Adjust rotation speed

    private bool isDragging = false;
    private float lastMouseX;
    [SerializeField] TMP_Text playerTitle;
    [Header("Player One")]
    [SerializeField] GameObject playerOne;
    [SerializeField] GameObject pOneColorPickers;
    [Header("Player Two")]
    [SerializeField] GameObject playerTwo;
    [SerializeField] GameObject pTwoColorPickers;
    /* public void SetRotation()
     {
         transform.rotation = Quaternion.Euler(0,slider.value,0);

         if (slider.value > 360)
             slider.value = 0;
         else if (slider.value < 0)
             slider.value = 360;
     }*/
    private void Start()
    {
        pTwoColorPickers.SetActive(false);
    }

    private void Update()
    {
        if (canRotate)
        {
            Vector3 mousePos = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMouseX = mousePos.x;
            }


            if (isDragging)
            {
                if (Input.GetMouseButton(0))
                {
                    float deltaX = mousePos.x - lastMouseX;
                    transform.Rotate(Vector3.up, -deltaX * rotationSpeed * Time.deltaTime);
                    lastMouseX = mousePos.x;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }
    public void ChangePlayer()
    {
        if (playerOne.activeSelf)
        {
            playerTitle.text = "Player Two";
            playerOne.SetActive(false);
            pOneColorPickers.SetActive(false);
            pTwoColorPickers.SetActive(true);
            playerTwo.SetActive(true);

            colorPickerUI.firstButtonNav.selectOnRight = colorPickerUI.colorPickerButton1p2.GetComponent<Button>();
            colorPickerUI.firstButtonNav.selectOnDown = colorPickerUI.finishButton.GetComponent<Button>();
            colorPickerUI.firstButton.GetComponent<Button>().navigation = colorPickerUI.firstButtonNav;

            colorPickerUI.finishButtonNav.selectOnLeft = colorPickerUI.firstButton.GetComponent<Button>();
            colorPickerUI.finishButtonNav.selectOnUp = colorPickerUI.colorPickerButton2p2.GetComponent<Button>();
            colorPickerUI.finishButton.GetComponent<Button>().navigation = colorPickerUI.finishButtonNav;
        }
        else
        {
            playerTitle.text = "Player One";
            playerTwo.SetActive(false);
            pTwoColorPickers.SetActive(false);
            pOneColorPickers.SetActive(true);
            playerOne.SetActive(true);

            colorPickerUI.firstButtonNav.selectOnRight = colorPickerUI.colorPickerButton1p1.GetComponent<Button>();
            colorPickerUI.firstButtonNav.selectOnDown = colorPickerUI.finishButton.GetComponent<Button>();
            colorPickerUI.firstButton.GetComponent<Button>().navigation = colorPickerUI.firstButtonNav;

            colorPickerUI.finishButtonNav.selectOnLeft = colorPickerUI.firstButton.GetComponent<Button>();
            colorPickerUI.finishButtonNav.selectOnUp = colorPickerUI.colorPickerButton2p1.GetComponent<Button>();
            colorPickerUI.finishButton.GetComponent<Button>().navigation = colorPickerUI.finishButtonNav;
        }
    }
    public void SetCanRotate(bool toggle)
    {
        canRotate = toggle;
    }
}
