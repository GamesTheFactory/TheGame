using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController_Fortnite : MonoBehaviour
{
    [Header("References")]
    public Transform target;          // Player
    public Transform cameraPivot;     // Se mueve con la posición del player
    public Transform bossTarget;

    private InputSystem_Actions input;

    [Header("Camera Settings")]
    public float sensitivity = 120f;
    public float minPitch = -25f;
    public float maxPitch = 65f;

    [Header("FOV")]
    public Camera cam;
    public float normalFOV = 74f;
    public float aimFOV = 62f;

    [Header("Distances")]
    public float normalDistance = 5f;
    public float aimDistance = 3.2f;
    public float epicDistance = 6f;
    public float distanceToBossToEpicView;

    [Header("Offsets (relative to pivot)")]
    public Vector3 normalOffset = new Vector3(1.3f, 0.30f, 0f);
    public Vector3 aimOffset = new Vector3(0.55f, 0.18f, 0f);
    public Vector3 epicOffset = new Vector3(0.0f, 0.45f, 0f);

    [Header("Collision")]
    public LayerMask collisionMask;
    public float sphereRadius = 0.25f;
    public float collisionSmooth = 0.05f;

    // Internals
    private bool isAiming = false;
    private float yaw;
    private float pitch;
    private Vector2 lookInput;

    private float currentDist;
    private float targetDist;

    private Vector3 currentOffset;
    private Vector3 targetOffset;

    private Vector3 pivotForward;  // Dirección suavizada del pivot

    private Vector2 smoothLook;
    private Vector2 smoothVelocity;
    public float lookSmooth = 0.05f;   // menor = más fluido (no lag) 


    // ---------------------------------------------
    void Awake()
    {
        input = new InputSystem_Actions();
    }

    void OnEnable()
    {
        input.Enable();

        input.Player.Look.performed += OnLook;
        input.Player.Look.canceled += OnLook;

        input.Player.Aiming.performed += ctx => isAiming = true;
        input.Player.Aiming.canceled += ctx => isAiming = false;
    }

    void OnDisable()
    {
        input.Player.Look.performed -= OnLook;
        input.Player.Look.canceled -= OnLook;

        input.Disable();
    }

    void Start()
    {
        if (!cam) cam = Camera.main;

        yaw = transform.eulerAngles.y;
        pitch = 10f;

        currentOffset = normalOffset;
        targetOffset = normalOffset;

        currentDist = normalDistance;
        targetDist = normalDistance;

        pivotForward = transform.forward;
    }

    // ---------------------------------------------
    void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    // ---------------------------------------------
    void LateUpdate()
    {
        if (!cameraPivot) return;

        FollowPlayer();     // << NUEVO: elimina jitter
        UpdateState();
        UpdateRotation();
        UpdatePosition();
        HandleCollision();
        UpdateFOV();
        AlignPlayer();      // << mover al final evita vibración
    }

    // ---------------------------------------------
    void FollowPlayer()
    {
        // CameraPivot sigue SOLO la posición del Player
        cameraPivot.position = target.position;
    }

    // ---------------------------------------------
    void UpdateState()
    {
        if (isAiming)
        {
            targetDist = aimDistance;
            targetOffset = aimOffset;
        }
        else
        {
            targetDist = normalDistance;
            targetOffset = normalOffset;

            if (bossTarget && Vector3.Distance(target.position, bossTarget.position) < distanceToBossToEpicView)
            {
                targetDist = epicDistance;
                targetOffset = epicOffset;
            }
        }

        currentDist = Mathf.Lerp(currentDist, targetDist, Time.deltaTime * 8f);
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * 10f);
    }

    // ---------------------------------------------
    void UpdateRotation()
    {
        // 1. Suavizar la lectura del ratón (no la rotación)
        smoothLook = Vector2.SmoothDamp(smoothLook, lookInput, ref smoothVelocity, lookSmooth);

        // 2. Aplicar suavizado a yaw/pitch
        yaw += smoothLook.x * sensitivity * Time.deltaTime;
        pitch -= smoothLook.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 3. Rotación directa ya no se ve a saltos
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        // 4. Pivot forward sin jitter
        pivotForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
    }


    // ---------------------------------------------
    void UpdatePosition()
    {
        Vector3 back = transform.rotation * Vector3.back;
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        Vector3 localOffset =
            right * currentOffset.x +
            up * currentOffset.y;

        Vector3 desiredPos = cameraPivot.position + localOffset + back * currentDist;

        transform.position = desiredPos;
    }

    // ---------------------------------------------
    void HandleCollision()
    {
        Vector3 start = cameraPivot.position;
        Vector3 dir = (transform.position - start).normalized;

        if (Physics.SphereCast(start, sphereRadius, dir, out RaycastHit hit, currentDist, collisionMask))
        {
            float fixedDist = Mathf.Max(0.5f, hit.distance - 0.15f);
            Vector3 newPos = start + dir * fixedDist;

            transform.position = Vector3.Lerp(transform.position, newPos, collisionSmooth);
        }
    }

    // ---------------------------------------------
    void UpdateFOV()
    {
        float fov = isAiming ? aimFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, Time.deltaTime * 10f);
    }

    // ---------------------------------------------
    void AlignPlayer()
    {
        if (!target) return;

        Vector3 dir = pivotForward;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        target.rotation = Quaternion.Slerp(target.rotation, rot, Time.deltaTime * 15f);
    }
}
