using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Transform orientation;
    Rigidbody body;
    private float startYScale;
    bool sprintToggle = false;
    bool crouchToggle = false;
    private float groundAngle;

    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float maxGroundAngle = 50;
    private float horizontalInput, verticalInput;
    private float moveSpeed;
    private Vector3 moveDirection;
    public MovementState movementState;

    [Header("Sprint Mode")]
    public bool ToggleSprintActive = false;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;

    [Header("Diving")]
    public float diveSpeed;

    [Header("Sliding")]
    public float slideSpeed;
    public float slideTimerMax;
    private float slideTimerCurrent;

    [Header("Ground Check")]
    public float playerHeight = 1;
    public Transform groundCheck;
    public LayerMask whatIsGround;
    RaycastHit groundHit;
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
        diving
    }

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
        startYScale = transform.localScale.y;
        moveSpeed = walkSpeed;
        movementState = MovementState.walking;
        

        slideTimerCurrent = 0;
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
        PlayerInput();
        SpeedControl();

        //Spaghetti to be removed later
        playerHeight = transform.localScale.y;
        //End spaghetti
    }

    private void FixedUpdate()
    {
        if(movementState != MovementState.diving)
            MovePlayer();
    }

    private void GroundCheck()
    {

        //WIP
/*        Physics.Raycast(transform.position, Vector3.down, out groundHit, playerHeight + 0.2f);

        //Debug.Log(groundHit.);

        if (groundHit.collider != null)
            grounded = true;

        if (Vector3.Angle(Vector3.up, groundHit.normal) < maxGroundAngle)
        {
                
        }
*/

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight + 0.2f, whatIsGround.value);
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            //Use invoke so we don't have to wait and interrupt any other input
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        StateHandler();
        SwitchCam();
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(body.velocity.x, 0f, body.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            body.velocity = new Vector3(limitedVel.x, body.velocity.y, limitedVel.z);
        }

        if (grounded)
            body.drag = groundDrag;
        else
            body.drag = 0;
    }

    private void MovePlayer()
    {
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        if (grounded)
            body.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        else
            body.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void DivePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        body.AddForce(moveDirection.normalized * diveSpeed, ForceMode.Impulse);
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

    private void StateHandler()
    {
        Vector3 flatVel = new Vector3(body.velocity.x, 0f, body.velocity.z);

        switch (movementState)
        {
            #region walking state
            case MovementState.walking:
                if (Input.GetKeyDown(sprintKey))
                {
                    if (ToggleSprintActive)
                        sprintToggle = true;

                    moveSpeed = sprintSpeed;

                    movementState = MovementState.sprinting;
                }
                else if (Input.GetKey(crouchKey))
                {
                    moveSpeed = crouchSpeed;
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    body.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                    movementState = MovementState.crouching;
                }
                else if (Input.GetKeyDown(crouchToggleKey))
                {
                    crouchToggle = true;
                    moveSpeed = crouchSpeed;
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    body.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                    movementState = MovementState.crouching;
                }
                else if (!grounded)
                {
                    movementState = MovementState.air;
                }
                break;
            #endregion
            #region sprinting state
            case MovementState.sprinting:

                if (ToggleSprintActive)
                {
                    if (Input.GetKeyDown(sprintKey) || flatVel.magnitude <= 0.9f * walkSpeed)
                    {
                        sprintToggle = false;
                        moveSpeed = walkSpeed;

                        movementState = MovementState.walking;
                    }
                }
                else
                {
                    if (!Input.GetKey(sprintKey) || flatVel.magnitude <= 0.9f * walkSpeed)
                    {
                        moveSpeed = walkSpeed;

                        movementState = MovementState.walking;
                    }
                }

                if (Input.GetKey(crouchKey))
                {
                    moveSpeed = slideSpeed;
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    body.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                    movementState = MovementState.sliding;
                }
                else if (Input.GetKeyDown(crouchToggleKey))
                {
                    crouchToggle = true;

                    moveSpeed = slideSpeed;
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    body.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                    movementState = MovementState.sliding;
                }
                else if (!grounded)
                {
                    movementState = MovementState.air;
                }
                break;
            #endregion
            #region crouching state
            case MovementState.crouching:

                if (crouchToggle)
                {
                    if (Input.GetKeyDown(crouchToggleKey) || Input.GetKeyDown(crouchKey)) {
                        crouchToggle = false;

                        moveSpeed = walkSpeed;
                        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

                        movementState = MovementState.walking;
                    }
                }
                else if (!crouchToggle)
                {
                    if (!Input.GetKey(crouchKey))
                    {
                        crouchToggle = false;

                        moveSpeed = walkSpeed;
                        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

                        movementState = MovementState.walking;
                    }
                }
                else if (!grounded)
                {
                    movementState = MovementState.diving;
                }
                break;
            #endregion
            #region sliding state
            case MovementState.sliding:
                slideTimerCurrent += Time.deltaTime;
                if(slideTimerCurrent >= slideTimerMax){
                    slideTimerCurrent = 0;
                    moveSpeed = crouchSpeed;
                    movementState = MovementState.crouching;
                }


                if (!crouchToggle && !Input.GetKey(crouchKey))
                {

                    transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

                    if (sprintToggle)
                    {
                        moveSpeed = sprintSpeed;

                        movementState = MovementState.sprinting;
                    }
                    else if (!sprintToggle)
                    { 
                        if (Input.GetKey(sprintKey))
                        {
                            moveSpeed = sprintSpeed;
                            movementState = MovementState.sprinting;
                        }
                        else
                        {
                            moveSpeed = walkSpeed;
                            movementState = MovementState.walking;
                        }
                    }
                }
                else if (!grounded)
                {
                    movementState = MovementState.diving;
                }
                break;
            #endregion
            #region air state
            case MovementState.air:
                if (Input.GetKeyDown(sprintKey))
                    sprintToggle = !sprintToggle;

                if (grounded)
                {
                    if (sprintToggle || Input.GetKey(sprintKey))
                    {
                        moveSpeed = sprintSpeed;
                        movementState = MovementState.sprinting;
                    }
                    else
                    {
                        moveSpeed = walkSpeed;
                        movementState = MovementState.walking;
                    }
                }
                else if (Input.GetKey(crouchKey))
                {
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    //Jeremy have dumb brain, transform.up * 0 probably very dumb. Jeremy dumb brain very sorry.
                    //On a serious note adding this line did create an issue with simulataneous jump + crouch input.
                    body.AddForce(transform.up * 0, ForceMode.VelocityChange);
                    moveSpeed = diveSpeed * 3;
                    DivePlayer();
                    movementState = MovementState.diving;
                }
                break;
            #endregion
            #region diving state
            case MovementState.diving:

                if(touchingWall && !grounded)
                {
                    transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                }
                if (grounded)
                {
                    if (flatVel.magnitude >= slideSpeed)
                    {
                        moveSpeed = slideSpeed;
                        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                        movementState = MovementState.sliding;
                    }
                    else
                    {
                        moveSpeed = crouchSpeed;
                        movementState = MovementState.crouching;
                    }

                }
                break;
            #endregion
        }
    }

    //Handle Wall bonks
    private bool touchingWall = false;

    void OnCollisionEnter(Collision other){
        if(other.gameObject.layer == LayerMask.NameToLayer("whatIsGround")){
            touchingWall = true;
        }
    }

    void OnCollisionExit(Collision other){
        if(other.gameObject.layer == LayerMask.NameToLayer("whatIsGround")){
            touchingWall = false;
        }
    }

}
