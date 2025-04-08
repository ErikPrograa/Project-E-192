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
    [SerializeField] float maxJumpHeight;
    [SerializeField] float maxJumpTime;
    [SerializeField] bool isJumping;
    [SerializeField] float maxFallingSpeed;
    // Gravity variables
    float gravity = -2f;
    float groundedGravity = -0.05f;

    [Header("Dash Variables")]
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashCooldown = 0.5f;

    [Header("Air Dash variables")]
    [SerializeField] int maxDashCount;
    [SerializeField] int dashCount;
    [SerializeField] float dashTimer;
    [SerializeField] float cooldownTimer;

    [Header("Timers")]
    [SerializeField] float timeToStartRunning;
    [SerializeField] float timeToLongFall;
    float currentTimeToLongFall;

    [Header("Player Flags")]
    bool isDashing;
    bool isGrounded;
    bool isLongFalling;
    bool canJump = true;
    bool isFalling;

    [Header("Movement Vectors | Values")]
    float targetVerticalVelocity;
    Vector3 targetHorizontalVelocity;
    Vector3 camForward;
    Vector3 camRight;
    Vector3 moveDirection;
    private void Awake()
    {
        audioSource = GetComponentInChildren(typeof(AudioSource)) as AudioSource;
        rb = GetComponent<Rigidbody>();
        currentTimeToLongFall = timeToLongFall;

        SetUpJumpVariables();

    }
    public Vector3 GetCurrentVelocity()
    {
        return rb.velocity;
    }
    void SetUpJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        maxFallingSpeed = -initialJumpVelocity;
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
        HandleTimers();
        HandleDash();
    }
    private void FixedUpdate()
    {
        UpdatePhysics();
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
        isGrounded = Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask);
    }
    private void HandleAnimations()
    {
        animationManager.SetAnimatorFloat("MovementSpeed", InputManager.Instance.movementInput.magnitude, 0.1f);
        animationManager.SetAnimatorBool("IsJumping", isJumping || !isGrounded);
        animationManager.SetAnimatorBool("IsLongFalling", isLongFalling);

    }
    private void HandleDash()
    {
        if (inputManager.isDashButtonPressed && dashCount > 0 && !isDashing && cooldownTimer <= 0f)
        {
            StartDash();
        }
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
            
        }
        else if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        Debug.Log(isDashing);
    }
    private void HandleGravity()
    {
        isFalling = rb.velocity.y <= 0.0f || !inputManager.isJumpButtonPressed;
        float fallMultiplier = 5.0f;
        if (isGrounded)
        {
            targetVerticalVelocity = groundedGravity;
            isLongFalling = false;

        }
        else if (isFalling)
        {
            float previousYVelocity = targetVerticalVelocity;
            float newYVelocity = targetVerticalVelocity + (gravity * fallMultiplier * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            targetVerticalVelocity = nextYVelocity;

        }
        else
        {
            float previousYVelocity = targetVerticalVelocity;
            float newYVelocity = targetVerticalVelocity + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            targetVerticalVelocity = nextYVelocity;
        }
    }
    void HandleMovement()
    {
        moveDirection = camForward * inputManager.movementInput.y + mainCamera.transform.right * inputManager.movementInput.x;
        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        if (!isDashing)
        {
            targetHorizontalVelocity = moveDirection * runningSpeed;
        }


    }
    void HandleJump()
    {
        if (!isJumping && isGrounded && inputManager.isJumpButtonPressed)
        {
            isJumping = true;
            targetVerticalVelocity = initialJumpVelocity * 0.5f;
        }
        else if (!inputManager.isJumpButtonPressed && isJumping && isGrounded)
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

    void HandleTimers()
    {
        if (isFalling)
        {
            currentTimeToLongFall -= Time.deltaTime;
            if (currentTimeToLongFall < 0 && !isLongFalling)
            {
                currentTimeToLongFall = timeToLongFall;
                isLongFalling = true;
            }
        }
    }
    void StartDash()
    {

        isDashing = true;
        dashTimer = dashDuration;
        dashCount--;

        targetHorizontalVelocity = moveDirection * runningSpeed* dashSpeed * Time.deltaTime;
    }
    void EndDash()
    {
        isDashing = false;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;
        dashCount = maxDashCount;
        
    }

    void UpdatePhysics()
    {
        float appliedVelocity = rb.velocity.y + targetVerticalVelocity;
        if(appliedVelocity < (maxFallingSpeed))
        {
            appliedVelocity = maxFallingSpeed;
        }
        rb.velocity = new Vector3(targetHorizontalVelocity.x, appliedVelocity, targetHorizontalVelocity.z);
        HandleGravity();
    }

    
}
