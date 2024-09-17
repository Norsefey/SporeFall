using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private float minFall = -1.5f;
    private float gravity = -9.81f;
    private float vertSpeed;
    private ControllerColliderHit contact;
 

    enum PlayerState
    {// two move states...so far, default and aiming
        Default, // free flow camera
        Aiming // player character rotates with camera
    }

    PlayerState currentState = PlayerState.Default;

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
                Debug.Log("aiming");
                AimingMovement();
                break;
        }
    }

    void DefaultMovement()
    {
        Vector3 movement = Vector3.zero;

        // old input system
        /* float horInput = Input.GetAxis("Horizontal");
         float verInput = Input.GetAxis("Vertical");*/
        // new Input System
        float horInput = pMan.moveAction.ReadValue<Vector2>().x;
        float verInput = pMan.moveAction.ReadValue<Vector2>().y;

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

        GravityHandler(movement);

        movement.y = vertSpeed;
        movement *= Time.deltaTime;
        cc.Move(movement);
        // Visual moves along with controller as separate object
        visual.position = transform.position;
        visual.forward = transform.forward;
    }
    public void JumpCall(InputAction.CallbackContext obj)
    {
        if (isGrounded())
        {
            vertSpeed = JumpSpeed;
        }
    }
    private void GravityHandler(Vector3 movement)
    {
        if (isGrounded())
            vertSpeed = minFall;
        else
        {
            // player falling speed
            vertSpeed += gravity * 5 * Time.deltaTime;
            if (vertSpeed < terminalVelocity)
            {
                vertSpeed = terminalVelocity;
            }

            if (cc.isGrounded)
            {
                if (Vector3.Dot(movement, contact.normal) < 0)
                {
                    movement = contact.normal * moveSpeed;
                }
                else
                {
                    movement += contact.normal * moveSpeed;
                }
            }
        }
    }
    private bool isGrounded()
    {
        bool hitground = false;
        RaycastHit hit;

        //checks if player is falling
        if (vertSpeed < 0.1 && Physics.Raycast(transform.position, Vector3.down, out hit))
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
        /// Old Input System
        /*float horInput = Input.GetAxis("Horizontal");
        float verInput = Input.GetAxis("Vertical");*/
        /// new Input system
        float horInput = pMan.moveAction.ReadValue<Vector2>().x;
        float verInput = pMan.moveAction.ReadValue<Vector2>().y;
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

        GravityHandler(movement);

        movement.y = vertSpeed;
        movement *= Time.deltaTime;
        cc.Move(movement);
        // visual moves with Character controller
        visual.position = transform.position;
    }
    public void SetAimState()
    {// called by TPS Camera to switch to aim state
        currentState = PlayerState.Aiming;
    }
    public void SetDefaultState()
    {// called by TPS Camera to switch to Default state
        currentState = PlayerState.Default;
    }
    public void SetManager(PlayerManager pManager)
    {
        this.pMan = pManager;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {// detects contact with ground
        contact = hit;
    }
}
