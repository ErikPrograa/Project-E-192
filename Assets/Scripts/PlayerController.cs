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
    int isJumpingHash;
    [SerializeField] float maxFallingSpeed;
    [SerializeField] int jumpCount;
    Dictionary<int,float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int,float> jumpGravities = new Dictionary<int, float>();
    [SerializeField] float secondJumpMultiplier;
    [SerializeField] float thirdJumpMultiplier;
    [SerializeField] float jumpResetTime;
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
    bool isMoving;
    public bool isDashing;
    bool isGrounded;
    bool isLongFalling;
    bool canJump = true;
    bool isFalling;
    bool isJumpAnimating;

    [SerializeField] Transform gravityTarget;

    [Header("Movement Vectors | Values")]
    Vector3 camForward;
    Vector3 camRight;
    Vector3 moveDirection;
    Vector3 appliedMovement;
    Vector3 currentMovement;

    [Header("Wall Run")]
    [SerializeField] float wallRunDuration = 3f;
    [SerializeField] LayerMask wallRunLayerMask;
    [SerializeField] float wallCheckDistance = 1f;
    [SerializeField] float wallRunGravity = -0.5f;
    [SerializeField] float wallRunSpeed = 6f;
    bool isWallRunning = false;
    [SerializeField] bool canWallRun;
    float wallRunTimer;
    Vector3 wallNormal;

    //Getters
    public bool IsGrounded() 
    {
        return isGrounded;
    }
    bool CheckWall(out RaycastHit hitInfo)
    {
        return Physics.Raycast(transform.position, transform.right, out hitInfo, wallCheckDistance, wallRunLayerMask) || Physics.Raycast(transform.position, -transform.right, out hitInfo, wallCheckDistance, wallRunLayerMask);
    }
    
    private void Awake()
    {
        audioSource = GetComponentInChildren(typeof(AudioSource)) as AudioSource;
        rb = GetComponent<Rigidbody>();
        currentTimeToLongFall = timeToLongFall;
        // Set the parameter hash references
        isJumpingHash = Animator.StringToHash("IsJumping");
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
        float secondJumpGravity = (-2 * (maxJumpHeight * secondJumpMultiplier)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (maxJumpHeight * secondJumpMultiplier)) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * (maxJumpHeight* thirdJumpMultiplier))/Mathf.Pow((timeToApex*1.5f),2);
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight * thirdJumpMultiplier)) / (timeToApex * 1.5f);

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
        isMoving = moveDirection != Vector3.zero;
        GetCameraDirections();
        GroundCheck();

        HandlePlayerRotation();
        HandleAnimations();
        HandleMovement();

        //Updates current movement

        HandleJump();

        isInteracting = isDashing;
        /*

        if (!isInteracting)
        {
            HandlePlayerRotation();
            HandleMovement();
            HandleJump();
            HandleGravity();
        }
        HandleAnimations();
        HandleTimers();
        HandleDash();
        if (canWallRun)
        {
            HandleWallRun();
        }
        */

    }
    private void FixedUpdate()
    {
        /*if (!isInteracting)
        {
            UpdatePhysics();
        }

        Debug.Log(rb.velocity);*/
        UpdatePhysics();
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
        isGrounded = Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask); 
        
    }
    private void HandleAnimations()
    {
        animationManager.SetAnimatorFloat("MovementSpeed", InputManager.Instance.movementInput.magnitude, 0.1f);
        animationManager.SetAnimatorBool("IsJumping", isJumpAnimating || !isGrounded);
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
    }
    private void HandleGravity()
    {
        bool isFalling = rb.velocity.y < 0.0f || !inputManager.isJumpButtonPressed;
        float fallMultiplier = 5f;
        if (isGrounded)
        {
            if (isJumpAnimating)
            {
                //animationManager.SetAnimatorBool("IsJumping", false);
                isJumpAnimating = false;
                if (jumpCount == 3)
                {
                    jumpCount = 0;

                }
            }
            currentMovement.y = groundedGravity;
        }
        else if(isFalling)
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
            float nextYVelocity = Mathf.Max((previousYVelocity+newYVelocity)*0.5f,-20.0f);
            currentMovement.y = nextYVelocity;

        }
        else
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (jumpGravities[jumpCount] * Time.deltaTime);
            float nextVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            currentMovement.y = nextVelocity;
        }
       /* if (canWallRun && isWallRunning)
        {
            currentMovement.y = wallRunGravity;
            return;
        }
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
            currentMovement.y = groundedGravity;


        }
        else if (isFalling)
        {
            float previousYVelocity = currentMovement.y;
            //float newYVelocity = targetVerticalVelocity + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
            //float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            //targetVerticalVelocity = nextYVelocity;
            currentMovement.y += gravity *Time.deltaTime;

        }
        else
        {
            isFalling = rb.velocity.y < groundedGravity || (!inputManager.isJumpButtonPressed && !isGrounded);
            float previousYVelocity = currentMovement.y;
            //float newYVelocity = targetVerticalVelocity + (jumpGravities[jumpCount] * Time.deltaTime);
            //float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            //targetVerticalVelocity = nextYVelocity;
            currentMovement.y += gravity *Time.deltaTime;

        }*/
        


    }
    void HandleMovement()
    {
        moveDirection = camForward * inputManager.movementInput.y + mainCamera.transform.right * inputManager.movementInput.x;
        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();
        Vector3 targetHorizontalVelocity = moveDirection * runningSpeed;
        currentMovement.x = targetHorizontalVelocity.x;
        currentMovement.z = targetHorizontalVelocity.z;


    }
    void HandleJump()
    {

        if (!isJumping && isGrounded && inputManager.isJumpButtonPressed)
        {
            if (jumpCount < 3 && currentJumpResetRoutine != null)
            {
                StopCoroutine(currentJumpResetRoutine);

            }
            isJumpAnimating = true;
            isJumping = true;
            jumpCount += 1;
            currentMovement.y = initialJumpVelocities[jumpCount] *0.5f;
            Debug.Log(currentMovement);
            /*if (jumpCount < 3 && currentJumpResetRoutine != null)
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
            currentMovement.y = initialJumpVelocity;*/

        }
        else if (!inputManager.isJumpButtonPressed && isJumping && isGrounded)
        {
            currentJumpResetRoutine = StartCoroutine(JumpResetRoutine());
            isJumping = false;
        }

    }
    #region Wall Run
    void StartWallRun()
    {
        isWallRunning = true;
        wallRunTimer = wallRunDuration;

        // Ignorar la gravedad fuerte al caer
        rb.useGravity = false;

        // Dar el impulso hacia adelante en direccion del movimiento (opcional)
        Vector3 forwardAlongWall = Vector3.Cross(wallNormal, Vector3.up);
        rb.velocity = forwardAlongWall * wallRunSpeed;
    }

    void HandleWallRun()
    {
        if(!isGrounded && !isWallRunning && rb.velocity.y <0f && inputManager.isJumpButtonPressed)
        {
            if(CheckWall(out RaycastHit hit))
            {
                wallNormal = hit.normal;
                StartWallRun();
            }
        }
        if (isWallRunning)
        {
            wallRunTimer -= Time.deltaTime;
            
            // Cancelar wall run si se acaba el tiempo o el jugador deja de presionar salto
            if(wallRunTimer <=0f || !inputManager.isJumpButtonPressed || isGrounded)
            {
                EndWallRun();
            }
        }
    }
    void EndWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;
    }
    #endregion
    IEnumerator JumpResetRoutine()
    {
        yield return new WaitForSeconds(jumpResetTime);
        jumpCount = 0;
    }
    void HandlePlayerRotation()
    {
        Vector3 positionToLookAt;
        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        Quaternion currentRotation = transform.rotation;
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime *rotationSpeed);
            
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
        /*float appliedVelocity = rb.velocity.y + targetVerticalVelocity;
        if(appliedVelocity < (maxFallingSpeed))
        {
            appliedVelocity = maxFallingSpeed;
        }*/

        float appliedYMovement = rb.velocity.y + currentMovement.y;
        rb.velocity = new Vector3(currentMovement.x, appliedYMovement, currentMovement.z);


    }

    
}
