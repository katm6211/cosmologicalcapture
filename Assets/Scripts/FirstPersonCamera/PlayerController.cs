using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Private Variables
    private Rigidbody rb;
    private Vector3 moveDirection;
    #endregion

    #region Public Variables
    public float groundDrag = 5f;
    public float moveSpeed = 5f;
    #endregion

    #region Ground Control 
    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    bool grounded;
    #endregion

    #region Jumping
    [Header("Jumping")]
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump = true;
    #endregion

    #region Keybinds
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    #endregion

    void Update()
    {
        // Raycast down from the player's center, slightly longer than half their height
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();

        // Drag only on the ground, none in the air
        rb.linearDamping = grounded ? groundDrag : 0f;

    }

    void SpeedControl() 
    {
        // Chose not to limit speed at this time
    }

    void MyInput()
    {
        // Calculate the movement based on pressed key WASD, and move the player accordingly
        moveDirection = PlayerCam.Instance.orientation.forward * Input.GetAxisRaw("Vertical") + PlayerCam.Instance.orientation.right * Input.GetAxisRaw("Horizontal");
        // Jump if space pressed & you're on the ground
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void Jump()
    {
        // Zero out vertical velocity first so every jump is the same height
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // freeze the rotation of the player so that it doesn't fall over when moving
    }
    void FixedUpdate()
    {
        MovePlayer(); // Apply physics based movement
    }

    private void MovePlayer()
    {
        // Move rigidbody by applying force at (some number) times the amplified movespeed in the direction of movedirection 
        if (grounded) 
            rb.AddForce(moveDirection.normalized * moveSpeed * 3f, ForceMode.Force);
        else // If in the air, add air multiplier 
            rb.AddForce(moveDirection.normalized * moveSpeed * 3f * airMultiplier, ForceMode.Force);
    }
}
