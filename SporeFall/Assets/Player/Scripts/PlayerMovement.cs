using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform visual;
    [SerializeField] private Transform myCamera;
    private PlayerManager pMan;
    private CharacterController cc;
    [Header("Movement Variables")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float rotSpeed = 15;
    private float moveSpeed;
    [Header("Jump Variables")]
    public float playerHeight = 2;
    public float JumpSpeed = 15;
    public float terminalVelocity = -10f;
    readonly float minFall = -1.5f;
    readonly float gravity = -9.81f;
    private float vertSpeed;
    //private ControllerColliderHit contact;
    private float tempAimTimer = 0;
    public enum PlayerState
    {// two move states...so far, default and aiming
        Default, // free flow camera
        Aiming, // player character rotates with camera
        TempAim
    }
    public PlayerState currentState = PlayerState.Default;
    private void Start()
    {
        cc = GetComponent<CharacterController>();
        moveSpeed = walkSpeed;
        SetDefaultState();
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
                if(tempAimTimer <= 0)
                {
                    if (currentState == PlayerState.TempAim)
                        currentState = PlayerState.Default;
                }
                break;
        }
    }
    public void RotateOnFire(Transform gun, Vector3 shootDir)
    {
        
        // convert player forward to camera forward
        Quaternion temp = myCamera.localRotation;
        // we only want the horizontal rotation of camera
        myCamera.eulerAngles = new Vector3(0, myCamera.eulerAngles.y, 0);
        // player moves with camera
        visual.forward = myCamera.forward;
        transform.forward = myCamera.forward;

        myCamera.localRotation = temp;
        // rotate the gun in direction of shot
        gun.forward = shootDir;
        // if player is moving while in default state
        if (cc.velocity.magnitude > 0)
        {
            // check if we are not already in tempAim state
            if (currentState != PlayerState.TempAim)
            {// if not enter temp aim state
                currentState = PlayerState.TempAim;
                tempAimTimer = 1.5f;
            }
            else if (currentState == PlayerState.TempAim)
            {// if we in temp state are just reset timer
                tempAimTimer = 1.5f;
            }
        }
    }
    void DefaultMovement()
    {
        Vector3 movement = Vector3.zero;
        // new Input System
        float horInput = pMan.pInput.moveAction.ReadValue<Vector2>().x;
        float verInput = pMan.pInput.moveAction.ReadValue<Vector2>().y;

        if (horInput != 0 || verInput != 0)
        {
            movement.x = horInput * moveSpeed;
            movement.z = verInput * moveSpeed;
            movement = Vector3.ClampMagnitude(movement, moveSpeed);
            // convert player forward to camera forward
            Quaternion temp = myCamera.localRotation;
            myCamera.eulerAngles = new Vector3(0, myCamera.eulerAngles.y, 0);
            movement = myCamera.TransformDirection(movement);
            myCamera.localRotation = temp;
            Quaternion direction = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);
        }

        GravityHandler();

        movement.y = vertSpeed;
        movement *= Time.deltaTime;
        cc.Move(movement);
        // Visual moves along with controller as separate object
        visual.position = transform.position;
        visual.forward = transform.forward;
    }
    public void JumpCall()
    {
        if (IsGrounded())
        {
            vertSpeed = JumpSpeed;
        }
    }
    private void GravityHandler()
    {
        if (IsGrounded())
            vertSpeed = minFall;
        else
        {
            // player falling speed
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

        //checks if player is falling
        if (vertSpeed < 0.1 && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            //Character height, slight below
            float check = playerHeight;
            hitground = hit.distance <= check;
        }

        return hitground && cc.isGrounded;
    }
    void AimingMovement()
    {
        Vector3 movement = Vector3.zero;
        /// new Input system
        float horInput = pMan.pInput.moveAction.ReadValue<Vector2>().x;
        float verInput = pMan.pInput.moveAction.ReadValue<Vector2>().y;
        // convert player forward to camera forward
        Quaternion temp = myCamera.localRotation;
        // we only want the horizontal rotation of camera
        myCamera.eulerAngles = new Vector3(0, myCamera.eulerAngles.y, 0);
        // player moves with camera
        visual.forward = myCamera.forward;
        transform.forward = myCamera.forward;
        if (horInput != 0 || verInput != 0)
        {
            // apply move speed
            movement.x = horInput * moveSpeed;
            movement.z = verInput * moveSpeed;
            // prevents going faster then set speed
            movement = Vector3.ClampMagnitude(movement, moveSpeed);
            movement = myCamera.TransformDirection(movement);
            Quaternion direction = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);
        }
        // restore camera
        myCamera.localRotation = temp;

        GravityHandler();

        movement.y = vertSpeed;
        movement *= Time.deltaTime;
        cc.Move(movement);
        // visual moves with Character controller
        visual.position = transform.position;
    }
    public void SetAimState()
    {// called by TPS Camera to switch to aim state
        currentState = PlayerState.Aiming;
        if (moveSpeed == sprintSpeed)
            moveSpeed = walkSpeed;
    }
    public void SetDefaultState()
    {// called by TPS Camera to switch to Default state
        currentState = PlayerState.Default;
    }
    public void SetManager(PlayerManager pManager)
    {
        this.pMan = pManager;
    }
    public void SetSprintSpeed(bool isSprinting)
    {
        if (isSprinting && currentState != PlayerState.Aiming)
            moveSpeed = sprintSpeed;
        else
            moveSpeed = walkSpeed;
    }
}
