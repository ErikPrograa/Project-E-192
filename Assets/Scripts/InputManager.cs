using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public PlayerInput playerInput;

    public Vector2 movementInput;
    public bool isJumpButtonPressed;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        playerInput = new PlayerInput();
    }
    private void OnEnable()
    {
        playerInput.Enable();
        playerInput.PlayerActions.Movement.performed += (ctx) => movementInput = ctx.ReadValue<Vector2>();
        playerInput.PlayerActions.Movement.canceled += (ctx) => movementInput = ctx.ReadValue<Vector2>();
        playerInput.PlayerActions.Jump.started += (ctx) => isJumpButtonPressed = true;
        playerInput.PlayerActions.Jump.canceled += (ctx) => isJumpButtonPressed = false;
    }

    
}
