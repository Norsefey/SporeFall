// Ignore Spelling: Gamepad

using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TPSCamera : MonoBehaviour
{
    [Header("references")]
    public Camera myCamera;
    [SerializeField] private PlayerMovement player;
    //[SerializeField] private CameraCollision camCollision;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float mHorSense = 50;
    [SerializeField] private float mvertSense = 50;

    [Header("Gamepad Sensitivity")]
    [SerializeField] private float gHorSense = 50;
    [SerializeField] private float gvertSense = 50;

    [Header("General Settings")]
    [SerializeField] float minVertRot = -45;
    [SerializeField] float maxVertRot = 45;
    [SerializeField] private bool invertVertRot = false;
    [SerializeField] private bool invertHorRot = false;
    [SerializeField] protected bool flipSide = false;

    [Header("Camera Offsets")]
    [SerializeField] Vector3 defaultOffset;
    [SerializeField] Vector3 aimOffset; // camera zooms in
    [SerializeField] Vector3 buildOffset; // camera zooms Out

    [Header("Collision Detection")]
    [SerializeField] private LayerMask obstructions;
    [SerializeField] private float minDistance = 1f; // Minimum distance between camera and player
    [SerializeField] private float cameraMoveThreshold = 0.05f;  // Small threshold to stop jitter
    [SerializeField] private float groundOffset = 0.5f;  // Offset to apply when colliding with the ground
    [SerializeField] private float groundDetectionAngle = 45f; // Angle to define what is considered 'ground'
    [SerializeField] private float collisionFreeTime = 0.5f;  // Time to wait before moving camera back
    private float timeSinceCollision = 0f;  // Time since last collision
    [SerializeField] private Transform aimTarget;
    [SerializeField] private LayerMask targetMask;
    // local private variables
    private PlayerManager pMan;
    private float vertRot = 0;
    private float horSense = 50;
    private float verSense = 50;

    private void Start()
    {
        myCamera.transform.localPosition = defaultOffset;
    }

    private void LateUpdate()
    {
        Ray ray = new(myCamera.transform.position, myCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, targetMask))
        {
            aimTarget.position = hit.point;
        }

            // moves camera set along with Character
            HolderMovement();
        // rotates camera based on mouse movement
        transform.localEulerAngles = new Vector3(VerticalRotation(), HorizontalRotation(), 0);
        if (player.currentState == PlayerMovement.PlayerState.Default)
            ObstructionCheck();
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
        // Player position adjusted for height
        Vector3 playerPos = player.transform.position + (Vector3.up * 2);

        // Desired position with default offset
        Vector3 desiredCameraPos = playerPos + transform.TransformDirection(defaultOffset);
        float currentDistance = defaultOffset.magnitude;

        // Check for obstructions using raycast
        if (Physics.Linecast(playerPos, desiredCameraPos, out RaycastHit hit, obstructions))
        {
            // If hit, reset the collision-free timer
            timeSinceCollision = 0f;

            // Calculate new camera position closer to the player
            float hitDistance = Mathf.Clamp(hit.distance, minDistance, currentDistance);
            Vector3 hitPoint = playerPos + (desiredCameraPos - playerPos).normalized * hitDistance;

            // Check if the hit is considered 'ground' based on the hit normal
            bool isGround = Vector3.Angle(hit.normal, Vector3.up) <= groundDetectionAngle;

            if (isGround)
            {
                // Apply ground offset if the hit object is the ground
                hitPoint.y += groundOffset;
            }

            // Move camera smoothly, but only if the difference is greater than the threshold
            if (Vector3.Distance(myCamera.transform.position, hitPoint) > cameraMoveThreshold)
            {
                myCamera.transform.position = Vector3.Lerp(myCamera.transform.position, hitPoint, Time.deltaTime * 10f);
            }
        }
        else
        {
            // If no collision, increment the time since last collision
            timeSinceCollision += Time.deltaTime;

            // Only move camera back to default if it has been free of collisions for some time
            if (timeSinceCollision >= collisionFreeTime)
            {
                // Smoothly move the camera back to the default offset
                Vector3 targetPosition = playerPos + transform.TransformDirection(defaultOffset);
                if (Vector3.Distance(myCamera.transform.position, targetPosition) > cameraMoveThreshold)
                {
                    myCamera.transform.position = Vector3.Lerp(myCamera.transform.position, targetPosition, Time.deltaTime * 5f);
                }
            }
        }

    }
    public void AimSight()
    {
        if(pMan.isBuilding)
            myCamera.transform.localPosition = buildOffset;
        else
            myCamera.transform.localPosition = aimOffset;

        //RemoveCollisionDetection();
        player.SetAimState();
    }
    public void DefaultSight()
    {
        myCamera.transform.localPosition = defaultOffset;

        //AssignCollisionDetection();
        player.SetDefaultState();
    }
    public void SetManager(PlayerManager pManager)
    {
        this.pMan = pManager;
    }
    public void SetMouseSettings()
    {
        horSense = mHorSense;
        verSense = mvertSense;
    }
    public void SetGamepadSettings()
    {
        horSense = gHorSense;
        verSense = gvertSense;
    }
    public void FlipCameraSide()
    {
        flipSide = !flipSide;
        
        defaultOffset.x *= -1;
        aimOffset.x *= -1;
        buildOffset.x *= -1;

        DefaultSight();
    }
}
