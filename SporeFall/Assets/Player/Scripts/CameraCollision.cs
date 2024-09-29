using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditorInternal.ReorderableList;

public class CameraCollision : MonoBehaviour
{
    public delegate void OnCameraCollision();
    public event OnCameraCollision onEnterCollision;
    public event OnCameraCollision onExitCollision;

    private void OnTriggerEnter(Collider other)
    {
        onEnterCollision?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        //GetComponent<Rigidbody>().velocity = Vector3.zero;

        onExitCollision?.Invoke();
    }
}
