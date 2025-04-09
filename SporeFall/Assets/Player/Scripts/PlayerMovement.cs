using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform visual;
    private Transform myCamera;
    private PlayerManager pMan;
    public CharacterController cc;

    [Header("Movement Variables")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float rotSpeed = 15;
    public bool isSprinting = false;
    private float moveSpeed;
    private float moveSpeedModifier = 1;

    [Header("Jump Variables")]
    public float playerHeight = 2;
    public float JumpSpeed = 15;
    public float terminalVelocity = -10f;
    readonly float minFall = -1.5f;
    readonly float gravity = -9.81f;
    private float vertSpeed;
    public float coyoteTime = 0.2f; // coyote time variable
    private float coyoteTimeCounter; // Counter to track coyote time

    private float tempAimTimer = 0;

    public enum PlayerState
    {
        Default,
        Aiming,
        TempAim,
        Immobile
    }
    public PlayerState currentState = PlayerState.Default;

    // Footstep audio variables
    [Header("Audio Settings")]
    public AudioClip footstepSound; // Footstep sound clip
    [Range(0f, 1f)] public float footstepVolume = 0.5f; // Volume of footstep sound
    public AudioSource audioSource;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
    }
    private void Start()
    {
        moveSpeed = walkSpeed;
        SetDefaultState();
        myCamera = pMan.pCamera.transform;
    }

    private void Update()
    {
        switch (currentState)
        {
            case PlayerState.Default:
                DefaultMovement();
                break;
            case PlayerState.Aiming:
                AimingMovement();
                break;
            case PlayerState.TempAim:
                AimingMovement();
                tempAimTimer -= Time.deltaTime;
                if (tempAimTimer <= 0)
                {
                    if (currentState == PlayerState.TempAim)
                        currentState = PlayerState.Default;
                }
                break;
            case PlayerState.Immobile:
                //Debug.Log("Player cannot Move");
                visual.position = transform.position - Vector3.up;
                visual.forward = transform.forward;
                break;
        }
    }
    public void RotateOnFire()
    {
        Quaternion temp = myCamera.localRotation;
        myCamera.eulerAngles = new Vector3(0, myCamera.eulerAngles.y, 0);
        transform.forward = myCamera.forward;
        visual.forward = myCamera.forward;

        myCamera.localRotation = temp;


        if (currentState != PlayerState.TempAim)
        {
            currentState = PlayerState.TempAim;
            tempAimTimer = 1.5f;
        }
        else if (currentState == PlayerState.TempAim)
        {
            tempAimTimer = 1.5f;
        }

    }
    void DefaultMovement()
    {
        Vector3 movement = Vector3.zero;
        float horInput = pMan.pInput.moveAction.ReadValue<Vector2>().x;
        float verInput = pMan.pInput.moveAction.ReadValue<Vector2>().y;

        if (horInput != 0 || verInput != 0)
        {
            movement.x = horInput * moveSpeed;
            movement.z = verInput * moveSpeed;
            movement = Vector3.ClampMagnitude(movement, moveSpeed * moveSpeedModifier);

            Quaternion temp = myCamera.localRotation;
            myCamera.eulerAngles = new Vector3(0, myCamera.eulerAngles.y, 0);
            movement = myCamera.TransformDirection(movement);
            myCamera.localRotation = temp;

            Quaternion direction = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);
            
            // Play footstep sound
            PlayFootstepSound();
        }

        GravityHandler();

        movement.y = vertSpeed;
        movement *= Time.deltaTime;
        cc.Move(movement);
        visual.position = transform.position - Vector3.up;
        visual.forward = transform.forward;
    }
    public void JumpCall()
    {
        if (IsGrounded() || coyoteTimeCounter > 0f)
        {
            vertSpeed = JumpSpeed;
            pMan.pAnime.ToggleFallingAnime(true);
            coyoteTimeCounter = 0f; // Reset coyote time counter
        }
    }
    private void GravityHandler()
    {
        if (IsGrounded())
        {
            pMan.pAnime.ToggleFallingAnime(false);
            vertSpeed = minFall;
            coyoteTimeCounter = coyoteTime; // Reset coyote time when grounded
        }
        else
        {
            // Decrement coyote time counter when not grounded
            coyoteTimeCounter -= Time.deltaTime;
            vertSpeed += gravity * 5 * Time.deltaTime;
            if (vertSpeed < terminalVelocity)
            {
                vertSpeed = terminalVelocity;
            }
        }
    }
    private bool IsGrounded()
    {
        bool hitground = false;

        if (vertSpeed < 0.1 && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, playerHeight))
        {
            float check = playerHeight;
            hitground = hit.distance <= check;

            pMan.inToxicWater = hit.collider.CompareTag("Water");
            moveSpeedModifier = pMan.inToxicWater ? pMan.slowDownMultiplier : 1;
        }

        return hitground && cc.isGrounded;
    }
    void AimingMovement()
    {
        Vector3 movement = Vector3.zero;
        float horInput = pMan.pInput.moveAction.ReadValue<Vector2>().x;
        float verInput = pMan.pInput.moveAction.ReadValue<Vector2>().y;

        Quaternion temp = myCamera.localRotation;
        myCamera.eulerAngles = new Vector3(0, myCamera.eulerAngles.y, 0);
        visual.forward = myCamera.forward;
        transform.forward = myCamera.forward;

        if (horInput != 0 || verInput != 0)
        {
            movement.x = horInput * moveSpeed;
            movement.z = verInput * moveSpeed;
            movement = Vector3.ClampMagnitude(movement, moveSpeed * moveSpeedModifier);
            movement = myCamera.TransformDirection(movement);
            Quaternion direction = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);
            
            // Play footstep sound
            PlayFootstepSound();
        }

        myCamera.localRotation = temp;

        GravityHandler();

        movement.y = vertSpeed;
        movement *= Time.deltaTime;
        cc.Move(movement);
        visual.position = transform.position - Vector3.up;
    }
    public void SetAimState()
    {
        currentState = PlayerState.Aiming;
        pMan.pAnime.ToggleIKAim(false);
        if (moveSpeed == sprintSpeed)
        {
            SetSprintSpeed(false);
        }
    }
    public void SetDefaultState()
    {
        pMan.pAnime.ToggleIKAim(true);

        currentState = PlayerState.Default;
    }
    public void SetManager(PlayerManager pManager)
    {
        this.pMan = pManager;
    }
    public void SetSprintSpeed(bool sprinting)
    {
        if (sprinting && currentState != PlayerState.Aiming)
        {
            moveSpeed = sprintSpeed;
            pMan.pAnime.ToggleSprint(true);
            isSprinting = true;
        }
        else
        {
            moveSpeed = walkSpeed;
            pMan.pAnime.ToggleSprint(false);
            isSprinting = false;

        }
    }
    private void PlayFootstepSound()
    {
        if (IsGrounded()) // Play footsteps only when grounded
        {
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(footstepSound, footstepVolume);
            }
        }
    }
}
