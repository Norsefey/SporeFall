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
    private float moveSpeed;
    public bool isSprinting = false;
    [Header("Jump Variables")]
    public float playerHeight = 2;
    public float JumpSpeed = 15;
    public float terminalVelocity = -10f;
    readonly float minFall = -1.5f;
    readonly float gravity = -9.81f;
    private float vertSpeed;

    private float tempAimTimer = 0;

    public enum PlayerState
    {
        Default,
        Aiming,
        TempAim
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
        moveSpeed = walkSpeed;
        SetDefaultState();
        myCamera = pMan.pCamera.transform;
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
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
        }
    }
    public void RotateOnFire(Transform gun, Vector3 shootDir)
    {
        Quaternion temp = myCamera.localRotation;
        myCamera.eulerAngles = new Vector3(0, myCamera.eulerAngles.y, 0);
        visual.forward = myCamera.forward;
        transform.forward = myCamera.forward;

        myCamera.localRotation = temp;

        if (cc.velocity.magnitude > 0)
        {
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
            movement = Vector3.ClampMagnitude(movement, moveSpeed);

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
        if (IsGrounded())
        {
            vertSpeed = JumpSpeed;
            pMan.pAnime.ToggleFallingAnime(true);

        }
    }

    private void GravityHandler()
    {
        if (IsGrounded())
        {
            pMan.pAnime.ToggleFallingAnime(false);
            vertSpeed = minFall;
        }
        else
        {
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

        if (vertSpeed < 0.1 && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            float check = playerHeight;
            hitground = hit.distance <= check;
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
            movement = Vector3.ClampMagnitude(movement, moveSpeed);
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
        if (moveSpeed == sprintSpeed)
        {
            SetSprintSpeed(false);
        }
    }

    public void SetDefaultState()
    {
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
        // Play footstep sound at intervals to avoid spamming
        if (!audioSource.isPlaying) // Ensure sound only plays if not already playing
        {
            audioSource.PlayOneShot(footstepSound, footstepVolume);
        }
    }
}
