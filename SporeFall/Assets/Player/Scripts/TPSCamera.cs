using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPSCamera : MonoBehaviour
{
    // references
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Transform hRot;
    [SerializeField] private Transform vRot;
    [SerializeField] private Transform cam;
    // horizontal rotations
    [SerializeField] private float horSense = 50;
    [SerializeField] private bool invertHorRot = false;
    // vertical rotations
    [SerializeField] private float verSense = 50;
    [SerializeField] private bool invertVertRot = false;
    [SerializeField] float minVertRot = -45;
    [SerializeField] float maxVertRot = 45;
    private float vertRot = 0;
    // aiming
    [SerializeField] Vector3 defaultOffset;
    [SerializeField] Vector3 aimOffset; // camera zooms in

    private void Update()
    {
        // moves camera set along with Character
        HolderMovement();
        // rotates camera based on mouse movement // update this to work with new Input System
        transform.localEulerAngles = new Vector3(VerticalRotation(), HorizontalRotation(), 0);

        // test code for aiming range weapon // update this to work with new Input System
        if (Input.GetMouseButton(1))
        {
            cam.transform.localPosition = aimOffset;
            player.SetAimState();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            cam.transform.localPosition = defaultOffset;
            player.SetDefaultState();
        }
    }

    void HolderMovement()
    {
        transform.position = player.transform.position;
    }
    float HorizontalRotation()
    {
        int invertedHor = invertHorRot ? -1 : 1;
        // update this to work with new Input System
        float xInput = Input.GetAxis("Mouse X") * horSense * invertedHor;
        float horRot = transform.localEulerAngles.y + xInput;
        return horRot;
    }
    float VerticalRotation()
    {
        int invertedVert = invertVertRot ? -1 : 1;
        // update this to work with new Input System
        vertRot -= Input.GetAxis("Mouse Y") * verSense * invertedVert;
        vertRot = Mathf.Clamp(vertRot, minVertRot, maxVertRot);
        return vertRot;
    }
}
