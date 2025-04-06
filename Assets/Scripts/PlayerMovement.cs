using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 4f;

    public float acceleration = 10f;
    public float deceleration = 15f;
    public float turnSpeed = 5f;
    public float pivotTurnSpeed = 15f;
    public float jumpForce = 10f;
    public float gravity = 20f;

    private CharacterController characterController;
    private Vector3 velocity;
    private float currentSpeed;

    private Vector3 lastMoveDirection;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }
    private void Update()
    {
        HandleMovement();
        ApplyGravity();
        characterController.Move(velocity*Time.deltaTime);
    }
    void HandleMovement()
    {
        
    }

    void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -1f;
        }
    }
}
