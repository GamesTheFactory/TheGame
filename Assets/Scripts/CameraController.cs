using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public Transform bossTarget;

    [Header("Settings")]
    public float mouseSensitivity = 120f;
    public float rotationSmoothTime = 0.05f;
    public float minPitch = -25f;
    public float maxPitch = 65f;

    [Header("Camera Distances")]
    public float normalDistance = 5f;
    public float aimDistance = 3.2f;
    public float bossEpicDistance = 6f;

    [Header("Offsets")]
    public Vector3 normalOffset = new Vector3(0, 1.6f, 0);
    public Vector3 aimOffset = new Vector3(0.5f, 1.6f, 0);
    public Vector3 bossEpicOffset = new Vector3(0, 2.2f, 0);

    [Header("Collision")]
    public LayerMask collisionMask;
    public float collisionRadius = 0.25f;
    public float collisionSmooth = 0.05f;

    [Header("Debug State")]
    public bool isAiming = false;

    // Private internals
    private InputSystem_Actions input;
    private float yaw;
    private float pitch;
    private Vector2 lookInput;
    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    private float currentDistance;
    private float targetDistance;
    private Vector3 targetOffset;

    // For AAA stabilization
    private Vector3 smoothedCamForward;

    private Vector3 currentOffset;


    void Awake()
    {
        input = new InputSystem_Actions();
    }
    void OnEnable()
    {
        input.Enable();
        input.Player.Look.performed += OnLook;
        input.Player.Look.canceled += OnLook;

        input.Player.Aim.performed += ctx => isAiming = true;
        input.Player.Aim.canceled += ctx => isAiming = false;
    }

    void OnDisable()
    {
        input.Player.Look.performed -= OnLook;
        input.Player.Look.canceled -= OnLook;
        input.Disable();
    }

    void Start()
    {
        currentOffset = normalOffset;

        yaw = transform.eulerAngles.y;
        pitch = 10f;

        currentDistance = normalDistance;
        targetDistance = normalDistance;
        targetOffset = normalOffset;

        smoothedCamForward = transform.forward;
    }

    void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    void LateUpdate()
    {
        if (!target) return;

        UpdateCameraState();
        UpdateRotation();
        UpdatePosition();
        HandleCollision();
    }

    // -------- CAMERA STATE ----------
    void UpdateCameraState()
    {
        // --- PRIORIDAD 1: AIMING ---
        if (isAiming)
        {
            targetDistance = aimDistance;
            targetOffset = aimOffset;
        }
        else
        {
            // --- PRIORIDAD 2: NORMAL ---
            targetDistance = normalDistance;
            targetOffset = normalOffset;

            // --- PRIORIDAD 3: MODO ÉPICO (solo si NO apuntas) ---
            if (bossTarget && Vector3.Distance(target.position, bossTarget.position) < 25f)
            {
                targetDistance = bossEpicDistance;
                targetOffset = bossEpicOffset;
            }
        }

        // --- INTERPOLACIÓN SUAVE (AAA) ---

        // Distancia suave
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 8f);

        // Offset suave
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * 10f);
    }



    // -------- ROTATION ----------
    void UpdateRotation()
    {
        yaw += lookInput.x * mouseSensitivity * Time.deltaTime;
        pitch -= lookInput.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Vector3 targetRot = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRot, ref rotationSmoothVelocity, rotationSmoothTime);

        transform.rotation = Quaternion.Euler(currentRotation);

        // AAA stabilization for player orientation
        Vector3 flat = transform.forward;
        flat.y = 0;
        flat.Normalize();

        smoothedCamForward = Vector3.Lerp(smoothedCamForward, flat, Time.deltaTime * 20f);
    }

    // -------- POSITION ----------
    void UpdatePosition()
    {
        Vector3 direction = transform.rotation * Vector3.back;
        Vector3 desiredPos = target.position + currentOffset + direction * currentDistance;


        transform.position = desiredPos;
    }

    // -------- COLLISION ----------
    void HandleCollision()
    {
        Vector3 start = target.position + targetOffset;
        Vector3 end = transform.position;

        Vector3 dir = (end - start).normalized;

        if (Physics.SphereCast(start, collisionRadius, dir, out RaycastHit hit, currentDistance, collisionMask))
        {
            float correctedDistance = Mathf.Max(0.5f, hit.distance - 0.2f);
            Vector3 correctedPos = start + dir * correctedDistance;

            transform.position = Vector3.Lerp(transform.position, correctedPos, collisionSmooth);
        }
    }

    // -------- PLAYER ALIGN ----------
    void FixedUpdate()
    {
        AlignPlayerWithCamera();
    }

    void AlignPlayerWithCamera()
    {
        if (!target) return;

        Vector3 dir = smoothedCamForward;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        target.rotation = Quaternion.Slerp(target.rotation, targetRot, Time.deltaTime * 12f);
    }
}

