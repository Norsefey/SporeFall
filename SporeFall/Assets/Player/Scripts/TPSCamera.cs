using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TPSCamera : MonoBehaviour
{
    // references
    private PlayerManager pMan;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Transform hRot;
    [SerializeField] private Transform vRot;
    [SerializeField] public Camera myCamera;
    // horizontal rotations
    [SerializeField] private float horSense = 50;
    [SerializeField] private bool invertHorRot = false;
    // vertical rotations
    [SerializeField] private float verSense = 50;
    [SerializeField] private bool invertVertRot = false;
    [SerializeField] float minVertRot = -45;
    [SerializeField] float maxVertRot = 45;
    private float vertRot = 0;

    [Header("Camera Offsets")]
    [SerializeField] Vector3 defaultOffset;
    [SerializeField] Vector3 aimOffset; // camera zooms in
    [SerializeField] Vector3 buildOffset; // camera zooms in

    private void Start()
    {
        myCamera.transform.localPosition = defaultOffset;
    }

    private void LateUpdate()
    {
        // moves camera set along with Character
        HolderMovement();
        // rotates camera based on mouse movement // update this to work with new Input System
        transform.localEulerAngles = new Vector3(VerticalRotation(), HorizontalRotation(), 0);
    }

    void HolderMovement()
    {
        transform.position = player.transform.position;
    }
    float HorizontalRotation()
    {
        int invertedHor = invertHorRot ? -1 : 1;
        /// New Input System
        float xInput = pMan.pInput.lookAction.ReadValue<Vector2>().x;
        float horRot = transform.localEulerAngles.y + xInput * horSense * invertedHor * Time.deltaTime;
        return horRot;
    }
    float VerticalRotation()
    {
        int invertedVert = invertVertRot ? -1 : 1;
        vertRot -= pMan.pInput.lookAction.ReadValue<Vector2>().y * verSense * invertedVert * Time.deltaTime;
        vertRot = Mathf.Clamp(vertRot, minVertRot, maxVertRot);
        return vertRot;
    }
    public void AimSightCall(InputAction.CallbackContext obj)
    {
        if(pMan.isBuilding)
            myCamera.transform.localPosition = buildOffset;
        else
            myCamera.transform.localPosition = aimOffset;
        player.SetAimState();
    }
    public void DefaultSightCall(InputAction.CallbackContext obj)
    {
        myCamera.transform.localPosition = defaultOffset;
        player.SetDefaultState();
    }
    public void SetManager(PlayerManager pManager)
    {
        this.pMan = pManager;
    }
}
