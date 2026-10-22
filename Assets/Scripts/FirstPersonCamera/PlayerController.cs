using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Private Variables
    private Rigidbody rb;
    private Vector3 moveDirection;
    #endregion
    #region Public Variables
    public float moveSpeed = 5f;
    #endregion

    void Update()
    {
        // Calculate the movement based on pressed key WASD, and move the player accordingly
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
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
        // Calculate target velocity while preserving existing vertical velocity and apply it to the rigidbody
        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; 
        rb.linearVelocity = targetVelocity;
    }
}
