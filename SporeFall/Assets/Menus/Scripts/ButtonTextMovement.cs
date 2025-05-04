using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTextMovement : MonoBehaviour
{
    public int offsetX = 2, offsetY = 2;
    public RectTransform textRect;
    Vector3 pos;
    public bool canMove = true;
    private bool textLocked = false;

    void Start()
    {
        pos = textRect.localPosition;
    }

    private void Update()
    {
        if (!canMove && textLocked == false)
        {
            textLocked = true;
            textRect.localPosition = new Vector3(pos.x + (float)offsetX, pos.y - (float)offsetY, pos.z);
        }
    }
    public void Down()
    {
        if (canMove)
        {
            textRect.localPosition = new Vector3(pos.x + (float)offsetX, pos.y - (float)offsetY, pos.z);
        }
        
    }

    public void Up()
    {
        if (canMove)
        {
            textRect.localPosition = pos;
        }
        
    }

    public void ControllerPressed()
    {
        if (canMove)
        {
            textRect.localPosition = new Vector3(pos.x + (float)offsetX, pos.y - (float)offsetY, pos.z);
            StartCoroutine(TextMoveDelay());
        }
        
    }

    IEnumerator TextMoveDelay()
    {
        Debug.Log("Starting delay timer");
        yield return new WaitForSecondsRealtime(.035f);
        textRect.localPosition = pos;
        Debug.Log("Text position reset");
    }
}
