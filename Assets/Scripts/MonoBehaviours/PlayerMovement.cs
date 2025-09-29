using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float maxSpeed = 12f;
    public float groundDrag = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float jumpCooldown = 0.2f;
    public float airMultiplier = 0.4f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public float groundCheckDistance = 0.4f; // Increased for better detection
    public LayerMask whatIsGround;
    
    [Header("References")]
    public Transform orientation;

    // Private variables
    private bool grounded;
    private bool readyToJump = true;
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
        
        // Visualize ground check in Scene view
        Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + groundCheckDistance), Color.red);
        
        // Get input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
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
        
        // Handle drag - FIXED: rb.drag instead of rb.linearDamping
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
        
        // Apply force
        float forceMultiplier = grounded ? 10f : 10f * airMultiplier;
        rb.AddForce(moveDirection.normalized * walkSpeed * forceMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        // Limit horizontal speed - FIXED: rb.velocity instead of rb.linearVelocity
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Reset y velocity for consistent jump height - FIXED: rb.velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        // Apply jump force
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}