using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    private CharacterController controller;
    private Vector3 velocity;

    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction jumpAction;
    public InputAction aimAction;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool aimingPressed;

    [Header("Animator")]
    public Animator anim;
    private string currentAnim = ""; // evita reproducir la misma animacion cada frame

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        aimAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        aimAction.Disable();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        ReadInput();
        HandleMovement();
        HandleJumpAndGravity();
        HandleAnimations();
    }

    // ------------------------------------------------------
    // INPUT
    // ------------------------------------------------------

    void ReadInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        jumpPressed = jumpAction.triggered;
        aimingPressed = aimAction.ReadValue<float>() > 0f;
    }

    // ------------------------------------------------------
    // MOVEMENT
    // ------------------------------------------------------

    void HandleMovement()
    {
        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    void HandleJumpAndGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (jumpPressed && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    // ------------------------------------------------------
    // ANIMATIONS (OPTIMIZED)
    // ------------------------------------------------------

    void HandleAnimations()
    {
        string nextAnim = "Idle";

        // PRIORIDAD 1: Aiming
        if (aimingPressed)
        {
            nextAnim = "Aiming";
        }
        else
        {
            // PRIORIDAD 2: Movimiento con WASD
            if (moveInput.y > 0.1f)
                nextAnim = "Player_Running";   // W
            else if (moveInput.y < -0.1f)
                nextAnim = "Run_Backward";     // S
            else if (moveInput.x > 0.1f)
                nextAnim = "Run_Right";        // D
            else if (moveInput.x < -0.1f)
                nextAnim = "Run_Left";         // A
        }

        // Si la animación ya está activa → NO reproducirla de nuevo
        if (currentAnim == nextAnim)
            return;

        anim.Play(nextAnim);
        currentAnim = nextAnim;
    }
}
