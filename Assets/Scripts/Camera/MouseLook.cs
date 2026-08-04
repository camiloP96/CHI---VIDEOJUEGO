using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Sensibilidad")]
    [Range(10f, 300f)]
    public float mouseSensitivity = 100f;

    [Header("Referencias")]
    public Transform playerBody;    // El cuerpo del personaje (rota horizontalmente)
    public Transform cameraTarget;  // El GameObject vacío detrás de la cabeza del jugador

    [Header("Suavizado")]
    public float smoothSpeed = 20f; // suavizar la camara que se tambalea

    [Header("Colisión con paredes")]
    public float cameraRadius = 0.2f;
    public LayerMask collisionMask;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // posición de la cámara al inicio
        if (cameraTarget != null)
        {
            transform.position = cameraTarget.position;
        }
    }

    void LateUpdate()
    {
        // INPUT DEL MOUSE 
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ROTACIÓN VERTICAL (cámara sube/baja) 
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // ROTACIÓN HORIZONTAL (personaje gira) 
        playerBody.Rotate(Vector3.up * mouseX);

        // POSICIÓN: seguimos al cameraTarget suavemente 
        if (cameraTarget != null)
        {
            // FIX PRINCIPAL: usamos cameraTarget.position, no playerBody.position
            // Así la cámara sigue el objeto vacío que pusiste detrás de la cabeza
            transform.position = Vector3.Lerp(
                transform.position,
                cameraTarget.position,
                smoothSpeed * Time.deltaTime
            );
        }

        // ROTACIÓN: combinamos la rotación del jugador + la vertical del mouse 
        // FIX: usamos Quaternion.Euler con la Y del playerBody para que la cámara
        // herede la rotación horizontal del personaje aunque no sea su hija
        transform.rotation = Quaternion.Euler(
            xRotation,                      // arriba/abajo (mouse)
            playerBody.eulerAngles.y,       // izquierda/derecha (hereda del jugador)
            0f
        );

        // COLISIÓN CON PAREDES (no funciona de momento)
        HandleCameraCollision();
    }

    void HandleCameraCollision()
    {
        if (cameraTarget == null) return;

        // Dirección desde el cameraTarget hacia la cámara
        Vector3 directionToCamera = transform.position - cameraTarget.position;
        float desiredDistance = directionToCamera.magnitude;

        // Si la distancia es casi 0 (cámara en primera persona), no hace falta el check
        if (desiredDistance < 0.01f) return;

        // SphereCast desde el cameraTarget hacia donde está la cámara
        if (Physics.SphereCast(
            cameraTarget.position,          // origen: el objeto detrás de la cabeza
            cameraRadius,
            directionToCamera.normalized,   // dirección hacia la cámara
            out RaycastHit hit,
            desiredDistance,
            collisionMask
        ))
        {
            // Si hay una pared, acercamos la cámara justo antes de ella
            float safeDistance = Mathf.Clamp(hit.distance - 0.05f, 0f, desiredDistance);
            transform.position = cameraTarget.position + directionToCamera.normalized * safeDistance;
        }
    }
}

