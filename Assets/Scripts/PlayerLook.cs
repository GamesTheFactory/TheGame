using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Sensibilidad del ratón")]
    public float mouseSensitivity = 100f;

    [Header("Referencias")]
    public Transform playerBody; // El cuerpo del jugador (para girar Y)
    public Transform playerCamera; // La cámara o cabeza (para girar X)

    private float xRotation = 0f; // Rotación vertical

    void Start()
    {
        // Bloquear el cursor al juego
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Obtener movimiento del ratón
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotación vertical (mirar arriba/abajo)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limitar para no girar completo

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotación horizontal (gira el cuerpo)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}

