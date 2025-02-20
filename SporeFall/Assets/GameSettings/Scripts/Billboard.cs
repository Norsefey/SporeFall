using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void LateUpdate()
    {
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
        }
            

        if (mainCamera != null)
        {
            // Make the UI element face the camera
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}
