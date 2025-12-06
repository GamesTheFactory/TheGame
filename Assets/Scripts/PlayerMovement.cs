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
    public InputAction moveAction;   // Vector2 (WASD / stick)
    public InputAction jumpAction;   // Button (space / gamepad A)

    private Vector2 moveInput;
    private bool jumpPressed;

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        ReadInput();
        HandleMovement();
        HandleJumpAndGravity();
    }

    void ReadInput()
    {
        // ⬇️ Lectura directa y eficiente del Input System
        moveInput = moveAction.ReadValue<Vector2>();
        jumpPressed = jumpAction.triggered;
    }

    void HandleMovement()
    {
        // Convertir input en dirección local
        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    void HandleJumpAndGravity()
    {
        // Cuando toca el suelo, reiniciamos velocidad vertical
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Salto
        if (jumpPressed && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Gravedad
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}


