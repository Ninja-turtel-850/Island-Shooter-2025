using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 1.5f;
    public float maxSpeed = 3f;
    public float groundDrag = 2.5f;

    [Header("Sprint Settings")]
    public float sprintMultiplier = 4f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Slow Walk Settings")]
    public float slowWalkMultiplier = 0.25f;
    public KeyCode slowWalkKey = KeyCode.LeftControl;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float jumpCooldown = 0.2f;
    public float airMultiplier = 0.4f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public float groundCheckDistance = 0.4f;
    public LayerMask whatIsGround;

    [Header("References")]
    public Transform orientation;

    // Private variables
    private bool grounded;
    private bool readyToJump = true;
    private bool isSprinting;
    private bool isSlowWalking;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + groundCheckDistance, whatIsGround);

        // Visualize ground check
        Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + groundCheckDistance), Color.red);

        // Get input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Sprint and slow-walk checks
        isSprinting = Input.GetKey(sprintKey) && grounded && !Input.GetKey(slowWalkKey);
        isSlowWalking = Input.GetKey(slowWalkKey) && grounded && !Input.GetKey(sprintKey);

        // Handle jump input
        if (Input.GetKeyDown(jumpKey))
        {
            if (readyToJump && grounded)
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }

        // Apply drag
        rb.linearDamping = grounded ? groundDrag : 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();
    }

    private void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Determine current speed based on state
        float currentSpeed = walkSpeed;
        if (isSprinting) currentSpeed = walkSpeed * sprintMultiplier;
        else if (isSlowWalking) currentSpeed = walkSpeed * slowWalkMultiplier;

        // Apply force
        float forceMultiplier = grounded ? 10f : 10f * airMultiplier;
        rb.AddForce(moveDirection.normalized * currentSpeed * forceMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        // Limit horizontal speed
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float currentMaxSpeed = maxSpeed;
        if (isSprinting) currentMaxSpeed = maxSpeed * sprintMultiplier;
        else if (isSlowWalking) currentMaxSpeed = maxSpeed * slowWalkMultiplier;

        if (flatVel.magnitude > currentMaxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMaxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Reset y velocity for consistent jump height
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Apply jump force
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
