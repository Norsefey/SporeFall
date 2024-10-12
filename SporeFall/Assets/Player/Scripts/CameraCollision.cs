using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public delegate void OnCameraCollision();
    public event OnCameraCollision OnEnterCollision;
    public event OnCameraCollision OnExitCollision;

    private void OnTriggerEnter(Collider other)
    {
        OnEnterCollision?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        //GetComponent<Rigidbody>().velocity = Vector3.zero;

        OnExitCollision?.Invoke();
    }
}
