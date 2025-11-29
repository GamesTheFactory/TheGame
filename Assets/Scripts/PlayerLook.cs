using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Sensibilidad del ratón")]
    public float mouseSensitivity = 100f;

    [Header("Suavizado")]
    public float smoothTime = 0.05f; // cuánto tarda en alcanzar la rotación deseada

    [Header("Referencias")]
    public Transform playerBody;    // Cuerpo del jugador
    public Transform playerCamera;  // Cámara del jugador

    private float xRotation = 0f;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1️⃣ Capturar movimiento del ratón
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity * Time.deltaTime;

        // 2️⃣ Suavizado
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, smoothTime);

        // 3️⃣ Rotación vertical (cámara)
        xRotation -= currentMouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 4️⃣ Rotación horizontal (cuerpo)
        playerBody.Rotate(Vector3.up * currentMouseDelta.x);
    }
}

