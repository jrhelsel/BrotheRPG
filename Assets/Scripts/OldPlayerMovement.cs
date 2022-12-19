using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldPlayerMovement : MonoBehaviour
{
    //public variables
    public Transform orientation;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Ground Check")]
    public float groundDistance;
    public Transform groundCheck;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    //private variables
    float horizontalInput, verticalInput;
    bool readyToJump = true;
    Vector3 moveDirection;
    Rigidbody body;

    public MovementState movementState;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air,
        aircrouching
    }

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

        PlayerInput();
        SpeedControl();
        StateHandler();

        if(grounded)
            body.drag = groundDrag;
        else
            body.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKey(sprintKey))
        {

        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            body.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey))
        {
            movementState = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        if(grounded && Input.GetKey(sprintKey))
        {
            movementState = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if(grounded)
        {
            movementState = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            movementState = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if(grounded)
            body.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            body.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(body.velocity.x, 0f, body.velocity.z);

        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            body.velocity = new Vector3(limitedVel.x, body.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        body.velocity = new Vector3(body.velocity.x, 0f, body.velocity.z);

        body.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
