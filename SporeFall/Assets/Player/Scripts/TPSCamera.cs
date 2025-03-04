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
    [SerializeField] private float offsetTransitionSpeed = 5f; // Controls how quickly camera transitions between states

    [Header("Camera Shake Settings")]
    [SerializeField] private float sprintShakeIntensity = 0.5f; // Increased from 0.15f
    [SerializeField] private float walkShakeIntensity = 0.25f; // Increased from 0.08f
    [SerializeField] private float shakeSpeed = 14f;
    [SerializeField] private float shakeSyncMultiplier = 0.5f; // Controls how in-sync horizontal and vertical shakes are
    [SerializeField] private float verticalShakeBias = 1.5f; // Makes vertical shake more pronounced
    [SerializeField] private float horizontalShakeBias = 1.2f; // Makes horizontal shake more pronounced
    [SerializeField] private float aimSteadiness = 0.4f; // Reduces shake while aiming
    private float horizontalShakeTime;
    private float verticalShakeTime;

    [Header("Improved Collision Settings")]
    [SerializeField] private LayerMask obstructions;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float sphereCastRadius = 0.2f;
    [SerializeField] private float collisionSmoothSpeed = 10f;
    [SerializeField] private float returnSmoothSpeed = 5f;
    [SerializeField] private float maxCheckDistance = 10f;
    [SerializeField] private float shoulderOffset = 0.5f; // Offset from center for better wall detection
    private Vector3 currentCameraOffset;
    private Vector3 smoothOffsetVelocity; // For offset transitions
    private Vector3 smoothPositionVelocity; // For position transitions
    private bool isColliding;

    private PlayerManager pMan;
    private float vertRot = 0;
    private float horRot = 0; // Added to track horizontal rotation
    private float horSense = 50;
    private float verSense = 50;
    private float angle = 0f;

    [SerializeField] private Transform rigLookAtTarget;

    private void Start()
    {
        if (player == null)
            player = pMan.pController;
        // Initialize the camera offset
        currentCameraOffset = defaultOffset;
    }

    private void LateUpdate()
    {
        if (player == null || pMan == null)
            return;
        // Handle state transitions
        HandleStateTransitions();
        // Calculate and apply rotation
        UpdateCameraRotation();
        // Check for obstructions between camera and player
        HandleCameraCollision();

        MoveRigTarget();
    }
    private void HandleStateTransitions()
    {
        // Determine target offset based on current state
        Vector3 targetOffset = GetCurrentOffsetForState();

        // Smoothly transition between offsets
        currentCameraOffset = Vector3.SmoothDamp(
            currentCameraOffset,
            targetOffset,
            ref smoothOffsetVelocity,
            Time.deltaTime * offsetTransitionSpeed
        );
    }
    private void UpdateCameraRotation()
    {
        // Calculate horizontal rotation
        int invertedHor = invertHorRot ? -1 : 1;
        float xInput = pMan.pInput.lookAction.ReadValue<Vector2>().x;
        horRot += xInput * horSense * invertedHor * Time.deltaTime;

        // Calculate vertical rotation
        int invertedVert = invertVertRot ? -1 : 1;
        float yInput = pMan.pInput.lookAction.ReadValue<Vector2>().y;
        vertRot -= yInput * verSense * invertedVert * Time.deltaTime;
        vertRot = Mathf.Clamp(vertRot, minVertRot, maxVertRot);

        // Calculate base rotation
        Vector3 baseRotation = new Vector3(vertRot, horRot, 0);

        // Add camera shake if moving
        if (player.cc.velocity.magnitude > 0.1f)
        {
            float currentShakeIntensity = CalculateShakeIntensity();
            Vector3 shakeOffset = CalculateShakeOffset(currentShakeIntensity);
            transform.eulerAngles = baseRotation + shakeOffset;
        }
        else
        {
            transform.eulerAngles = baseRotation;
        }
    }
    private void HandleCameraCollision()
    {
        // Get a position on the player's shoulder (using world space)
        Vector3 shoulderPosition = player.transform.position + transform.right * (flipSide ? -shoulderOffset : shoulderOffset);

        // Calculate the desired camera position from the shoulder with the current offset
        Vector3 desiredCameraPosition = shoulderPosition + transform.TransformDirection(currentCameraOffset);

        // Cast a sphere from the player's shoulder to the desired camera position
        RaycastHit hit;
        Vector3 directionToCamera = (desiredCameraPosition - shoulderPosition).normalized;
        float distanceToCamera = Vector3.Distance(shoulderPosition, desiredCameraPosition);

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
            Debug.Log(hit.collider.gameObject.name);
            // Calculate adjusted position based on hit point
            float adjustedDistance = hit.distance - sphereCastRadius;
            Vector3 newPosition = shoulderPosition + directionToCamera * adjustedDistance;

            // Update position with collision
            transform.position = Vector3.SmoothDamp(
                transform.position,
                newPosition,
                ref smoothPositionVelocity,
                Time.deltaTime * collisionSmoothSpeed
            );
            isColliding = true;
        }
        else
        {
            // Move toward the desired position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredCameraPosition,
                ref smoothPositionVelocity,
                Time.deltaTime * (isColliding ? collisionSmoothSpeed : returnSmoothSpeed)
            );

            // Check if we've returned to the desired position
            if (Vector3.Distance(transform.position, desiredCameraPosition) < 0.01f)
            {
                isColliding = false;
            }
        }

        // Make sure camera is looking at the right direction
        LookAtPlayer();
    }
    private void MoveRigTarget()
    {
        Vector3 shootDirection = myCamera.transform.forward;
        
        Ray ray = new(transform.position, shootDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, 100, obstructions)) // Range of the hitscan weapon
        {
            rigLookAtTarget.position = hit.point;
        }
        else
        {
            rigLookAtTarget.position = shootDirection * 100;
        }
    }
    private void LookAtPlayer()
    {
        // Create a temporary target point slightly above the player for better framing
        Vector3 targetPoint = player.transform.position + Vector3.up * 0.5f;

        // Create a look rotation but only extract the vertical component
        Quaternion lookRot = Quaternion.LookRotation(targetPoint - transform.position);

        // Apply our calculated rotations manually
        transform.eulerAngles = new Vector3(vertRot, horRot, 0);
    }
    private Vector3 GetCurrentOffsetForState()
    {
        if (player.currentState == PlayerMovement.PlayerState.Aiming)
        {
            return pMan.isBuilding ? buildOffset : aimOffset;
        }
        return defaultOffset;
    }
    public void AimSight()
    {
        // Update player state
        player.SetAimState();
    }
    public void DefaultSight()
    {
        // Update player state
        player.SetDefaultState();
    }
    public IEnumerator PanAroundPlayer(Transform target, float duration, float orbitSpeed)
    {
        // Store original camera state
        float elapsedTime = 0f;
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;
        Vector3 originalOffset = currentCameraOffset;
        bool wasColliding = isColliding;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime since time is frozen

            // Increment angle for rotation
            angle += orbitSpeed * Time.unscaledDeltaTime;

            // Calculate new position around the player
            float x = target.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * 5;
            float z = target.position.z + Mathf.Sin(angle * Mathf.Deg2Rad) * 5;
            float y = transform.position.y; // Maintain current height

            // Update camera position
            transform.position = new Vector3(x, y, z);

            // Keep looking at the player
            transform.LookAt(target);

            yield return null;
        }

        // Restore camera state with smooth transition
        StartCoroutine(SmoothlyRestoreCamera(originalPosition, originalRotation, originalOffset, wasColliding));

    }
    private IEnumerator SmoothlyRestoreCamera(Vector3 position, Quaternion rotation, Vector3 offset, bool wasColliding)
    {
        float duration = 0.5f; // Transition duration
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startOffset = currentCameraOffset;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Use smoothstep for more natural easing
            float smoothT = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, position, smoothT);
            transform.rotation = Quaternion.Slerp(startRot, rotation, smoothT);
            currentCameraOffset = Vector3.Lerp(startOffset, offset, smoothT);

            yield return null;
        }

        // Ensure final values are set precisely
        transform.position = position;
        transform.rotation = rotation;
        currentCameraOffset = offset;
        isColliding = wasColliding;
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
        float speed = player.cc.velocity.magnitude;

        // Determine base intensity based on movement speed with a more dynamic range
        if (player.isSprinting)
        {
            // Scale intensity with speed for more natural feel
            baseIntensity = Mathf.Lerp(walkShakeIntensity, sprintShakeIntensity, Mathf.Clamp01(speed / 8f));
        }
        else
        {
            // Scale intensity with speed for walking too
            baseIntensity = Mathf.Lerp(0.1f, walkShakeIntensity, Mathf.Clamp01(speed / 4f));
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
        // Increment shake timers at different rates for horizontal and vertical
        horizontalShakeTime += Time.deltaTime * shakeSpeed;
        verticalShakeTime += Time.deltaTime * shakeSpeed * shakeSyncMultiplier;

        // Create separate noise patterns for horizontal and vertical shake
        // The 0.5f offset creates a more natural bobbing motion
        float xShake = (Mathf.PerlinNoise(horizontalShakeTime, 0.5f) * 2.0f - 1.0f) * intensity * horizontalShakeBias;
        float yShake = (Mathf.PerlinNoise(0.5f, verticalShakeTime) * 2.0f - 1.0f) * intensity * verticalShakeBias;

        // Add a slight rhythmic component to match footsteps
        float footstepFrequency = player.isSprinting ? 7.0f : 4.5f;
        float footstepBounce = Mathf.Sin(horizontalShakeTime * footstepFrequency) * 0.3f * intensity;
        yShake += footstepBounce;

        // Apply a subtle roll effect for more natural camera movement
        float zShake = xShake * 0.15f;

        return new Vector3(yShake, xShake, zShake);
    }
}
