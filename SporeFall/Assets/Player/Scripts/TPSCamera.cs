// Ignore Spelling: Gamepad

using System.Collections;
using UnityEngine;
public class TPSCamera : MonoBehaviour
{
    [Header("references")]
    public Camera myCamera;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private AudioListener audioListener;
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

    [Header("Camera Shake Settings")]
    [SerializeField] private float sprintShakeIntensity = 0.15f;
    [SerializeField] private float walkShakeIntensity = 0.08f;
    [SerializeField] private float shakeSpeed = 14f;
    [SerializeField] private float aimSteadiness = 0.4f; // Reduces shake while aiming
                                                         // Local private variables for shake
    private float shakeTime;
    [Header("Improved Collision Settings")]
    [SerializeField] private float sphereCastRadius = 0.2f;
    [SerializeField] private float collisionSmoothSpeed = 10f;
    [SerializeField] private float returnSmoothSpeed = 5f;
    [SerializeField] private float maxCheckDistance = 10f;
    [SerializeField] private float shoulderOffset = 0.5f; // Offset from center for better wall detection

    // Local private variables
    private Vector3 currentCameraOffset;
    private Vector3 targetCameraOffset;
    private Vector3 smoothVelocity;
    private bool isColliding;
    [Header("Collision Detection")]
    [SerializeField] private LayerMask obstructions;
    [SerializeField] private float minDistance = 1f; // Minimum distance between camera and player
    [SerializeField] private float cameraMoveThreshold = 0.05f;  // Small threshold to stop jitter
    [SerializeField] private float groundOffset = 0.5f;  // Offset to apply when colliding with the ground
    [SerializeField] private float groundDetectionAngle = 45f; // Angle to define what is considered 'ground'
    [SerializeField] private float collisionFreeTime = 0.5f;  // Time to wait before moving camera back
    [SerializeField] private Transform aimTarget;
    [SerializeField] private LayerMask targetMask;
    
    // local private variables
    private PlayerManager pMan;
    private float vertRot = 0;
    private float horSense = 50;
    private float verSense = 50;
    private float angle = 0f;
    private void Start()
    {
        myCamera.transform.localPosition = defaultOffset;
    }

    private void LateUpdate()
    {
        // moves camera set along with Character
        HolderMovement();
        // Calculate base rotation
        Vector3 baseRotation = new Vector3(VerticalRotation(), HorizontalRotation(), 0);

        // Add camera shake if moving
        if (player.cc.velocity.magnitude > 0.1f)
        {
            float currentShakeIntensity = CalculateShakeIntensity();
            Vector3 shakeOffset = CalculateShakeOffset(currentShakeIntensity);
            transform.localEulerAngles = baseRotation + shakeOffset;
        }
        else
        {
            transform.localEulerAngles = baseRotation;
        }

        if (player.currentState == PlayerMovement.PlayerState.Default)
        {
            //ObstructionCheck();
            ImprovedObstructionCheck();
        }
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
    private void ImprovedObstructionCheck()
    {
        // Get the desired camera position based on current offset
        Vector3 targetOffset = player.currentState == PlayerMovement.PlayerState.Aiming ? aimOffset : defaultOffset;

        // Calculate camera desired position with shoulder offset
        Vector3 shoulderPosition = transform.position + (transform.right * (flipSide ? -shoulderOffset : shoulderOffset));
        Vector3 desiredPosition = shoulderPosition + transform.TransformDirection(targetOffset);

        // Cast a sphere from the player's shoulder to the desired camera position
        RaycastHit hit;
        Vector3 directionToCamera = (desiredPosition - shoulderPosition).normalized;
        float distanceToCamera = Vector3.Distance(shoulderPosition, desiredPosition);

        bool hasCollision = Physics.SphereCast(
            shoulderPosition,
            sphereCastRadius,
            directionToCamera,
            out hit,
            Mathf.Min(distanceToCamera, maxCheckDistance),
            obstructions
        );

        if (hasCollision)
        {
            // Calculate new position based on hit point
            float adjustedDistance = hit.distance - sphereCastRadius;
            Vector3 newPosition = shoulderPosition + directionToCamera * adjustedDistance;

            // Convert world position to local offset
            Vector3 newOffset = transform.InverseTransformPoint(newPosition) - transform.InverseTransformPoint(shoulderPosition);

            // Update target offset with collision position
            targetCameraOffset = newOffset;
            isColliding = true;
        }
        else if (isColliding)
        {
            // Smoothly return to default position when no collision
            targetCameraOffset = targetOffset;
            if (Vector3.Distance(currentCameraOffset, targetCameraOffset) < 0.01f)
            {
                isColliding = false;
            }
        }

        // Smooth camera movement
        float smoothSpeed = isColliding ? collisionSmoothSpeed : returnSmoothSpeed;
        currentCameraOffset = Vector3.SmoothDamp(
            currentCameraOffset,
            targetCameraOffset,
            ref smoothVelocity,
            Time.deltaTime * smoothSpeed
        );

        // Apply the offset to the camera
        myCamera.transform.localPosition = currentCameraOffset;
    }

    public void AimSight()
    {
        if(pMan.isBuilding)
            myCamera.transform.localPosition = buildOffset;
        else
            myCamera.transform.localPosition = aimOffset;

        player.SetAimState();
    }
    public void DefaultSight()
    {
        myCamera.transform.localPosition = defaultOffset;

        player.SetDefaultState();
    }
    public IEnumerator PanAroundPlayer(Transform target, float duration, float orbitSpeed)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime since time is frozen

            // Increment angle for rotation
            angle += orbitSpeed * Time.unscaledDeltaTime;

            // Calculate new position around the player
            float x = target.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * 5;
            float z = target.position.z + Mathf.Sin(angle * Mathf.Deg2Rad) * 5;

            // Update camera position
            transform.position = new Vector3(x, transform.position.y, z);

            // Keep looking at the player
            transform.LookAt(target);

            yield return null;
        }
    }
    public void DisableAudioListener()
    {
        Debug.Log("Disabling Audio Listener");
        audioListener.enabled = false;
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

    private float CalculateShakeIntensity()
    {
        float baseIntensity;

        // Determine base intensity based on movement speed
        if (player.isSprinting)
        {
            baseIntensity = sprintShakeIntensity;
        }
        else
        {
            baseIntensity = walkShakeIntensity;
        }

        // Reduce shake while aiming
        if (player.currentState == PlayerMovement.PlayerState.Aiming)
        {
            baseIntensity *= aimSteadiness;
        }

        return baseIntensity;
    }

    private Vector3 CalculateShakeOffset(float intensity)
    {
        shakeTime += Time.deltaTime * shakeSpeed;

        // Create procedural shake using perlin noise for smooth random movement
        float xShake = (Mathf.PerlinNoise(shakeTime, 0.0f) * 2.0f - 1.0f) * intensity;
        float yShake = (Mathf.PerlinNoise(0.0f, shakeTime) * 2.0f - 1.0f) * intensity;

        return new Vector3(xShake, yShake, 0);
    }
}
