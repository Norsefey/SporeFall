using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]

    [SerializeField] Transform visual;
    [SerializeField] private Transform myCamera;
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
    private float _vertSpeed;
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

        float horInput = Input.GetAxis("Horizontal");
        float verInput = Input.GetAxis("Vertical");

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

        // Jump Stuff
        bool hitground = false;
        RaycastHit hit;
        //checks if player is falling
        if (_vertSpeed < 0.1 && Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            //Character height, slight below
            float check = playerHeight;
            hitground = hit.distance <= check;
        }
        if (hitground && cc.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _vertSpeed = JumpSpeed;
            }
            else
            {
                _vertSpeed = minFall;
            }
        }
        else
        {
            _vertSpeed += gravity * 5 * Time.deltaTime;
            if (_vertSpeed < terminalVelocity)
            {
                _vertSpeed = terminalVelocity;
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

        movement.y = _vertSpeed;
        movement *= Time.deltaTime;
        cc.Move(movement);
        // apply transforms to visuals
        visual.position = transform.position;
        visual.forward = transform.forward;
    }
    void AimingMovement()
    {
        Vector3 movement = Vector3.zero;
        float horInput = Input.GetAxis("Horizontal");
        float verInput = Input.GetAxis("Vertical");

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

        // Jump Stuff
        bool hitground = false;
        RaycastHit hit;

        //checks if player is falling
        if (_vertSpeed < 0.1 && Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            //Character height, slight below
            float check = playerHeight;
            hitground = hit.distance <= check;
        }

        if (hitground && cc.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _vertSpeed = JumpSpeed;
            }
            else
            {
                _vertSpeed = minFall;
            }
        }
        else
        {
            _vertSpeed += gravity * 5 * Time.deltaTime;
            if (_vertSpeed < terminalVelocity)
            {
                _vertSpeed = terminalVelocity;
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

        movement.y = _vertSpeed;
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
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {// detects contact with ground
        contact = hit;
    }
}
