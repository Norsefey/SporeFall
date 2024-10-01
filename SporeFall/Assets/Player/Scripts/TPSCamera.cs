using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TPSCamera : MonoBehaviour
{
    // references
    public Camera myCamera;
    private PlayerManager pMan;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private CameraCollision camCollision;
    // horizontal rotations
    [SerializeField] private float horSense = 50;
    [SerializeField] private bool invertHorRot = false;
    // vertical rotations
    [SerializeField] private float verSense = 50;
    [SerializeField] private bool invertVertRot = false;
    [SerializeField] float minVertRot = -45;
    [SerializeField] float maxVertRot = 45;
                     private float vertRot = 0;
    [SerializeField] private LayerMask obstructions;

    [Header("Camera Offsets")]
    [SerializeField] Vector3 defaultOffset;
    [SerializeField] Vector3 aimOffset; // camera zooms in
    [SerializeField] Vector3 buildOffset; // camera zooms in
    private void Start()
    {
        myCamera.transform.localPosition = defaultOffset;
        camCollision.transform.localPosition = defaultOffset;
        AssignCollisionDetection();
    }

    private void LateUpdate()
    {
        ObstructionCheck();
        // moves camera set along with Character
        HolderMovement();
        // rotates camera based on mouse movement
        transform.localEulerAngles = new Vector3(VerticalRotation(), HorizontalRotation(), 0);
    }
    private void HolderMovement()
    {
        transform.position = player.transform.position;
    }
    private float HorizontalRotation()
    {
        int invertedHor = invertHorRot ? -1 : 1;
        /// New Input System
        float xInput = pMan.pInput.lookAction.ReadValue<Vector2>().x;
        float horRot = transform.localEulerAngles.y + xInput * horSense * invertedHor * Time.deltaTime;
        return horRot;
    }
    private float VerticalRotation()
    {
        int invertedVert = invertVertRot ? -1 : 1;
        vertRot -= pMan.pInput.lookAction.ReadValue<Vector2>().y * verSense * invertedVert * Time.deltaTime;
        vertRot = Mathf.Clamp(vertRot, minVertRot, maxVertRot);
        return vertRot;
    }
    private void ObstructionCheck()
    {
        // Perform a raycast from the target towards the camera's desired position to check for obstacles
        Vector3 playerOffset = player.transform.position + Vector3.up;
        Vector3 rayDirection = (playerOffset - camCollision.transform.position).normalized;
        float rayDistance = Vector3.Distance(playerOffset, camCollision.transform.position);

        if (Physics.Raycast(camCollision.transform.position, rayDirection, rayDistance, obstructions))
        {
            // If there's a collision, place the camera closer to the hit point to avoid clipping
            CollisionAdjustCamera();
            //desiredPosition = hit.point - rayDirection * 0.1f; // Push camera slightly off from the hit point
        }
    }
    private void CollisionAdjustCamera()
    {
        myCamera.transform.localPosition = aimOffset;
    }
    private void CollisionDefaultPosition()
    {
        myCamera.transform.localPosition = defaultOffset;
        camCollision.transform.localPosition = defaultOffset;
    }
    private void AssignCollisionDetection()
    {
        camCollision.onEnterCollision += CollisionAdjustCamera;
        camCollision.onExitCollision += CollisionDefaultPosition;
    }
    private void RemoveCollisionDetection()
    {
        camCollision.onEnterCollision -= CollisionAdjustCamera;
        camCollision.onExitCollision -= CollisionDefaultPosition;
    }
    public void AimSight()
    {
        if(pMan.isBuilding)
            myCamera.transform.localPosition = buildOffset;
        else
            myCamera.transform.localPosition = aimOffset;

        RemoveCollisionDetection();
        player.SetAimState();
    }
    public void DefaultSight()
    {
        myCamera.transform.localPosition = defaultOffset;

        AssignCollisionDetection();
        player.SetDefaultState();
    }
    public void SetManager(PlayerManager pManager)
    {
        this.pMan = pManager;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 playerOffset = player.transform.position + Vector3.up;
        Vector3 rayDirection = (playerOffset - camCollision.transform.position).normalized;
        rayDirection *= Vector3.Distance(playerOffset, camCollision.transform.position);
        Gizmos.DrawRay(camCollision.transform.position, rayDirection);
    }
}
