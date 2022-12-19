using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Transform orientation;
    Rigidbody body;
    private float startYScale;

    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    private float horizontalInput, verticalInput;
    private float moveSpeed;
    private Vector3 moveDirection;
    public MovementState movementState;
    private MovementState previousMovementState;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;

    [Header("Sliding")]
    public float slideSpeed;

    [Header("Ground Check")]
    public float groundDistance;
    public Transform groundCheck;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Camera")]
    public GameObject cam_1P;
    public GameObject cam_3P;
    private bool inFirstPerson;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode crouchToggleKey = KeyCode.C;
    public KeyCode changePerspective = KeyCode.P;





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
        moveSpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

        PlayerInput();
        SpeedControl();
        SwitchCam();

        if (grounded)
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

        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        StateHandler();
    }

    private void StateHandler()
    {
        switch(movementState)
        {
            case MovementState.walking:
                if (Input.GetKeyDown(sprintKey))
                {
                    moveSpeed = sprintSpeed;

                    movementState = MovementState.sprinting;
                    previousMovementState = MovementState.walking;
                }
                else if (Input.GetKey(crouchKey))
                {
                    moveSpeed = crouchSpeed;
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    body.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                    movementState = MovementState.crouching;
                    previousMovementState = MovementState.walking;
                }
                else if (Input.GetKeyDown(crouchToggleKey))
                {
                    moveSpeed = crouchSpeed;

                    movementState = MovementState.crouching;
                    previousMovementState = MovementState.walking;
                }
                else if (!grounded)
                {
                    movementState = MovementState.air;
                    previousMovementState = MovementState.walking;
                }
                break;

            case MovementState.sprinting:
                Vector3 flatVel = new Vector3(body.velocity.x, 0f, body.velocity.z);

                if (Input.GetKeyDown(sprintKey) || flatVel.magnitude <= 0.9f * walkSpeed)
                {
                    moveSpeed = walkSpeed;

                    movementState = MovementState.walking;
                    previousMovementState = MovementState.sprinting;
                }
                else if (Input.GetKey(crouchKey))
                {
                    moveSpeed = slideSpeed;
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    body.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                    movementState = MovementState.sliding;
                    previousMovementState = MovementState.sprinting;
                }
                else if (Input.GetKeyDown(crouchToggleKey))
                {
                    moveSpeed = slideSpeed;
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    body.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                    movementState = MovementState.sliding;
                    previousMovementState = MovementState.sprinting;
                }
                else if (!grounded)
                {
                    movementState = MovementState.air;
                    previousMovementState = MovementState.sprinting;
                }
                break;

            case MovementState.crouching:
                if (!Input.GetKey(crouchKey))
                {
                    moveSpeed = walkSpeed;
                    transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

                    movementState = MovementState.walking;
                    previousMovementState = MovementState.crouching;
                }
                else if (!grounded)
                {
                    movementState = MovementState.aircrouching;
                    previousMovementState = MovementState.crouching;
                }
                break;

            case MovementState.sliding:
                if (!Input.GetKey(crouchKey))
                {
                    transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

                    if (Input.GetKey(sprintKey))
                        movementState = MovementState.sprinting;
                    else
                        movementState = MovementState.walking;
                }
                else if (!grounded)
                {
                    movementState = MovementState.aircrouching;
                }
                break;

            case MovementState.air:
                if (grounded)
                {
                    if (previousMovementState == MovementState.walking)
                    {
                        movementState = MovementState.walking;
                    }
                    else if (previousMovementState == MovementState.sprinting)
                    {
                        movementState = MovementState.sprinting;
                    }
                }
                else if (Input.GetKey(crouchKey))
                {
                    movementState = MovementState.aircrouching;
                }
                break;

            case MovementState.aircrouching:
                if (grounded)
                {
                    moveSpeed = crouchSpeed;

                    movementState = MovementState.crouching;
                }
                else if (!Input.GetKey(crouchKey))
                {
                    movementState = MovementState.air;
                }
                break;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
            body.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            body.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(body.velocity.x, 0f, body.velocity.z);

        if (flatVel.magnitude > moveSpeed)
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

    private void SwitchCam()
    {
        if (Input.GetKeyDown(changePerspective))
        {
            if (inFirstPerson)
            {
                cam_1P.SetActive(false);
                cam_3P.SetActive(true);

                inFirstPerson = false;
            }
            else
            {
                cam_1P.SetActive(true);
                cam_3P.SetActive(false);

                inFirstPerson = true;
            }
        }
    }
}
