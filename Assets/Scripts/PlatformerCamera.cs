using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerCamera : MonoBehaviour
{
    [Header("Referencia al Objetivo")]
    public Transform target; // Objeto a seguir (por ejemplo, el jugador)
    public Vector3 offset;

    [Header("Parámetros de la Cámara")]
    public float distance = 5.0f;      // Distancia ideal desde el objetivo
    public float height = 2.0f;        // Altura del punto de pivote sobre el objetivo
    public float rotationSpeed = 5.0f; // Velocidad de giro (input del mouse)
    public float smoothSpeed = 10f;    // Suavizado de movimientos

    [Header("Control de Ángulo Vertical")]
    public float minPitch = -40f; // Ángulo mínimo (mirar hacia abajo)
    public float maxPitch = 85f;  // Ángulo máximo (mirar hacia arriba)

    [Header("Ajustes de Colisión de Cámara")]
    public float maxDistance = 5.0f;     // Distancia máxima (igual que 'distance')
    public float minDistance = 1.0f;     // Distancia mínima para no acercarse demasiado
    public float collisionRadius = 0.3f; // Radio del SphereCast para detectar colisiones
    public LayerMask collisionLayer;     // Capas que pueden colisionar con la cámara

    // Variables internas para el manejo de la cámara
    private float currentAngle = 0f;   // Ángulo horizontal (yaw)
    private float currentPitch = 10f;  // Ángulo vertical inicial (pitch)
    private float currentDistance;     // Distancia actual de la cámara (ajustada según colisión)

    // Variables para capturar la entrada del mouse (opcional)
    float rotationX;
    float rotationY;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotationY = angles.y;
        rotationX = angles.x;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        // Inicializamos currentDistance con la distancia ideal
        currentDistance = distance;
    }

    private void Update()
    {
        // Aquí podrías agregar lógica adicional si fuese necesario.
    }

    private void FixedUpdate()
    {
        // Se suele trabajar la física en FixedUpdate, pero en este caso toda la lógica se aplica en LateUpdate.
    }

    private void LateUpdate()
    {
        // Actualiza el ángulo horizontal según el input del mouse
        currentAngle += Input.GetAxis("Mouse X") * rotationSpeed;
        // Actualiza el ángulo vertical (invertido para sensación natural)
        currentPitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // Crea la rotación compuesta a partir del pitch y yaw
        Quaternion rotation = Quaternion.Euler(currentPitch, currentAngle, 0);

        // Define el punto de pivote (por ejemplo, la posición del target con un offset vertical)
        Vector3 pivot = target.position + offset;

        // Posición ideal de la cámara sin considerar colisiones
        Vector3 idealPosition = pivot - rotation * Vector3.forward * distance;

        // Calcula la dirección desde el pivote hacia la posición ideal (esta será la dirección del SphereCast)
        Vector3 direction = (idealPosition - pivot).normalized;

        // Inicialmente, usamos la distancia ideal
        float desiredDistance = distance;

        // Emite un SphereCast desde el pivote en la dirección del idealPosition
        RaycastHit hit;
        if (Physics.SphereCast(pivot, collisionRadius, direction, out hit, distance, collisionLayer))
        {
            // Si se detecta un obstáculo, se ajusta la distancia para colocar la cámara justo antes del mismo
            desiredDistance = Mathf.Clamp(hit.distance, minDistance, distance);
        }

        // Interpolar suavemente la distancia actual hacia la deseada para evitar cambios bruscos
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * smoothSpeed);

        // Calcula la posición final de la cámara usando la distancia ajustada
        Vector3 finalPosition = pivot - rotation * Vector3.forward * currentDistance;

        // Aplica suavizado en la posición de la cámara
        transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * smoothSpeed);

        // La cámara siempre mira hacia el pivote
        transform.LookAt(pivot);
    }
}