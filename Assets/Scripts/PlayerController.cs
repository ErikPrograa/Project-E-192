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

    [Header("Jump variables")]
    [SerializeField] float initialJumpVelocity;
    [SerializeField] float maxJumpHeight;
    [SerializeField] float maxJumpTime;
    public bool isJumping;
    [SerializeField] float maxFallingSpeed;
    [SerializeField] int jumpCount;
    Dictionary<int,float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int,float> jumpGravities = new Dictionary<int, float>();
    Coroutine currentJumpResetRoutine = null;


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
    bool isInteracting;
    public bool isDashing;
    bool isGrounded;
    bool isLongFalling;
    bool canJump = true;
    bool isFalling;
    bool isJumpAnimating;

    //Getters
    public bool IsGrounded() 
    {
        return isGrounded;
    }

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
        float secondJumpGravity = (-2 * (maxJumpHeight + 2)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (maxJumpHeight + 2)) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * (maxJumpHeight+4))/Mathf.Pow((timeToApex*1.5f),2);
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight + 4)) / (timeToApex * 1.5f);

        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2,secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        jumpGravities.Add(0, gravity);
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);



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
        if (!isInteracting)
        {
            HandleMovement();
            HandlePlayerRotation();
            HandleJump();
        }
        HandleAnimations();
        HandleTimers();
        HandleDash();
        isInteracting = isDashing;
        Debug.Log("Is Falling " + isFalling);
        Debug.Log("Is LongFalling " + isLongFalling);
        Debug.Log(currentTimeToLongFall);
    }
    private void FixedUpdate()
    {
        if (!isInteracting)
        {
            UpdatePhysics();
        }
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
        animationManager.SetAnimatorBool("IsJumping", isJumping && !isGrounded);
        animationManager.SetAnimatorBool("IsLongFalling", isLongFalling);
        animationManager.SetAnimatorBool("IsFalling", isFalling);

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
        float fallMultiplier = 5.0f;
        if (isGrounded)
        {
            isLongFalling = false;
            isFalling= false;
            currentTimeToLongFall = timeToLongFall;
            if (isJumpAnimating)
            {
                currentJumpResetRoutine = StartCoroutine(JumpResetRoutine());
                isJumpAnimating = false;
            }
            targetVerticalVelocity = groundedGravity;


        }
        else if (isFalling)
        {
            float previousYVelocity = targetVerticalVelocity;
            float newYVelocity = targetVerticalVelocity + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            targetVerticalVelocity = nextYVelocity;

        }
        else
        {
            isFalling = rb.velocity.y < groundedGravity || (!inputManager.isJumpButtonPressed && !isGrounded);
            float previousYVelocity = targetVerticalVelocity;
            float newYVelocity = targetVerticalVelocity + (jumpGravities[jumpCount] * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            targetVerticalVelocity = nextYVelocity;
        }
        
        
        
    }
    void HandleMovement()
    {
        moveDirection = camForward * inputManager.movementInput.y + mainCamera.transform.right * inputManager.movementInput.x;
        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();
        targetHorizontalVelocity = moveDirection * runningSpeed;


    }
    void HandleJump()
    {
        if (!isJumping && isGrounded && inputManager.isJumpButtonPressed)
        {

            if (jumpCount < 3 && currentJumpResetRoutine != null)
            {
                StopCoroutine(currentJumpResetRoutine);

            }
            else if (jumpCount >= 3 && currentJumpResetRoutine != null)
            {
                StopCoroutine(currentJumpResetRoutine);
                jumpCount = 0;
            }
            jumpCount += 1;
            isJumping = true;
            isJumpAnimating = true;
            targetVerticalVelocity = initialJumpVelocities[jumpCount] * 0.5f;

        }
        else if (!inputManager.isJumpButtonPressed && isJumping && isGrounded)
        {
            isJumping = false;
        }

    }
    IEnumerator JumpResetRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        jumpCount = 0;
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
            if(!isLongFalling)
            {
                currentTimeToLongFall -= Time.deltaTime;

                if (currentTimeToLongFall < 0)
                {
                    currentTimeToLongFall = timeToLongFall;
                    isLongFalling = true;
                }
            }
            
        }
    }
    void StartDash()
    {
        rb.velocity = Vector3.zero;

        isDashing = true;
        dashTimer = dashDuration;
        dashCount--;
        Vector3 dashDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);
        rb.AddForce(dashDirection * runningSpeed * dashSpeed);
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
