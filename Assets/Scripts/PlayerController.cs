using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    AnimationManager animationManager;
    AudioSource audioSource;

    [Header("References")]
    InputManager inputManager;
    Rigidbody rb;
    [SerializeField] Camera mainCamera;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheckTransform;
    [SerializeField] float groundCheckRadius;
    [SerializeField] LayerMask groundCheckLayerMask;

    [Header("Player variables")]
    [SerializeField] float runningSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float rotationSpeed;

    [SerializeField] float initialJumpVelocity;
    [SerializeField] float maxJumpHeight= 1.0f;
    [SerializeField] float maxJumpTime= 0.5f;
    [SerializeField] bool isJumping;
    // Gravity variables
    [SerializeField] float gravity = -2f;
    [SerializeField] float groundedGravity = -0.05f;

    [Header("Timers")]
    [SerializeField] float timeToStartRunning;
    [Header("Player Flags")]
    bool isGrouded;
    bool canJump =true;


    [Header("Movement Vectors | Values")]
    float targetVerticalVelocity;
    Vector3 targetHorizontalVelocity;
    Vector3 camForward;
    Vector3 camRight;
    private void Awake()
    {
        audioSource = GetComponentInChildren(typeof(AudioSource)) as AudioSource;
        rb= GetComponent<Rigidbody>();
        SetUpJumpVariables();
    }

    void SetUpJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }
    private void Start()
    {
        inputManager = InputManager.Instance;
        animationManager = GetComponentInChildren<AnimationManager>();
    }
    private void Update()
    {
        GroundCheck();
        GetCameraDirections();
        HandleMovement();
        HandlePlayerRotation();
        HandleAnimations();
        HandleJump();

        Debug.Log(targetVerticalVelocity);
    }
    private void FixedUpdate()
    {
        UpdatePhysics();
    }
    private void HandleAnimations()
    {
        animationManager.SetAnimatorFloat("MovementSpeed",InputManager.Instance.movementInput.magnitude,0.1f);
    }
    private void HandleGravity()
    {
        if (isGrouded)
        {
            targetVerticalVelocity = groundedGravity;

        }
        else
        {
            targetVerticalVelocity += gravity * Time.deltaTime;
        }
    }
    void HandleMovement()
    {
        Vector3 moveDirection = camForward*inputManager.movementInput.y + mainCamera.transform.right*inputManager.movementInput.x;
        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        targetHorizontalVelocity = moveDirection*runningSpeed*Time.deltaTime;
    }
    void HandleJump()
    {
        if(!isJumping && isGrouded && inputManager.isJumpButtonPressed)
        {
            isJumping = true;
            targetVerticalVelocity = initialJumpVelocity;
        }
        else if(!inputManager.isJumpButtonPressed && isJumping && isGrouded)
        {
            isJumping = false;

        }
    }
    void HandlePlayerRotation()
    {
        if (targetHorizontalVelocity.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetHorizontalVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

    }
    void UpdatePhysics()
    {
        rb.velocity = new Vector3(targetHorizontalVelocity.x, rb.velocity.y +targetVerticalVelocity, targetHorizontalVelocity.z);
        HandleGravity();
    }

    void GetCameraDirections()
    {
        camForward = mainCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        camRight = mainCamera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

    }

    void GroundCheck()
    {
        isGrouded = Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask);
    }
}
