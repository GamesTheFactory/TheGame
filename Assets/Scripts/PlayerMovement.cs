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
    private string currentAnim = "";

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
        AlignMovementDirection();
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

        // ARREGLADO → esto funciona SIEMPRE
        aimingPressed = aimAction.IsPressed();
    }

    // ------------------------------------------------------
    // MOVEMENT
    // ------------------------------------------------------

    void HandleMovement()
    {
        // Reducir velocidad un 25% cuando se está apuntando
        float finalSpeed = aimingPressed ? moveSpeed * 0.5f : moveSpeed;

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        controller.Move(move * finalSpeed * Time.deltaTime);
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
    // ANIMATIONS (OPTIMIZED + AIMING FIXED)
    // ------------------------------------------------------

    void HandleAnimations()
    {
        string nextAnim = "Idle";

        bool isMoving =
            Mathf.Abs(moveInput.x) > 0.1f ||
            Mathf.Abs(moveInput.y) > 0.1f;

        // --------------------------
        // 1. AIMING LOGIC
        // --------------------------

        if (aimingPressed)
        {
            if (!isMoving)
            {
                // Aiming quieto
                nextAnim = "Aiming";
            }
            else
            {
                // Aiming + movimiento → usar animaciones de correr
                if (moveInput.y > 0.1f)
                    nextAnim = "Player_Running";   // W
                else if (moveInput.y < -0.1f)
                    nextAnim = "Run_Backward";     // S
                else if (moveInput.x > 0.1f)
                    nextAnim = "Run_Right";        // D
                else if (moveInput.x < -0.1f)
                    nextAnim = "Run_Left";         // A
            }
        }
        else
        {
            // --------------------------
            // 2. LÓGICA NORMAL (sin Aiming)
            // --------------------------

            if (moveInput.y > 0.1f)
                nextAnim = "Player_Running";
            else if (moveInput.y < -0.1f)
                nextAnim = "Run_Backward";
            else if (moveInput.x > 0.1f)
                nextAnim = "Run_Right";
            else if (moveInput.x < -0.1f)
                nextAnim = "Run_Left";
        }

        // --------------------------
        // 3. PREVENIR REPRODUCIR MISMA ANIMACIÓN
        // --------------------------

        if (currentAnim == nextAnim)
            return;

        anim.Play(nextAnim);
        currentAnim = nextAnim;
    }

    void AlignMovementDirection()
    {
        // No rotar si no hay movimiento
        if (moveInput.sqrMagnitude < 0.1f)
            return;

        // Solo diagonales hacia delante
        if (moveInput.y > 0.1f && Mathf.Abs(moveInput.x) > 0.1f)
        {
            Vector3 moveDir = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDir.y = 0;

            if (moveDir.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            float rotateSpeed = 15f;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
        }

        // Todo lo demás: no gira
    }


}
