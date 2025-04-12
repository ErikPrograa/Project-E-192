using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerCamera : MonoBehaviour
{
    [Header("Referencia al Objetivo")]
    public Transform target; // Objeto a seguir (por ejemplo, el jugador)
    public Vector3 offset;

    [Header("Par�metros de la C�mara")]
    public float distance = 5.0f;      // Distancia ideal desde el objetivo
    public float height = 2.0f;        // Altura del punto de pivote sobre el objetivo
    public float rotationSpeed = 5.0f; // Velocidad de giro (input del mouse)
    public float smoothSpeed = 10f;    // Suavizado de movimientos

    [Header("Control de �ngulo Vertical")]
    public float minPitch = -40f; // �ngulo m�nimo (mirar hacia abajo)
    public float maxPitch = 85f;  // �ngulo m�ximo (mirar hacia arriba)

    [Header("Ajustes de Colisi�n de C�mara")]
    public float maxDistance = 5.0f;     // Distancia m�xima (igual que 'distance')
    public float minDistance = 1.0f;     // Distancia m�nima para no acercarse demasiado
    public float collisionRadius = 0.3f; // Radio del SphereCast para detectar colisiones
    public LayerMask collisionLayer;     // Capas que pueden colisionar con la c�mara

    // Variables internas para el manejo de la c�mara
    private float currentAngle = 0f;   // �ngulo horizontal (yaw)
    private float currentPitch = 10f;  // �ngulo vertical inicial (pitch)
    private float currentDistance;     // Distancia actual de la c�mara (ajustada seg�n colisi�n)

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
        // Aqu� podr�as agregar l�gica adicional si fuese necesario.
    }

    private void FixedUpdate()
    {
        // Se suele trabajar la f�sica en FixedUpdate, pero en este caso toda la l�gica se aplica en LateUpdate.
    }

    private void LateUpdate()
    {
        // Actualiza el �ngulo horizontal seg�n el input del mouse
        currentAngle += Input.GetAxis("Mouse X") * rotationSpeed;
        // Actualiza el �ngulo vertical (invertido para sensaci�n natural)
        currentPitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // Crea la rotaci�n compuesta a partir del pitch y yaw
        Quaternion rotation = Quaternion.Euler(currentPitch, currentAngle, 0);

        // Define el punto de pivote (por ejemplo, la posici�n del target con un offset vertical)
        Vector3 pivot = target.position + offset;

        // Posici�n ideal de la c�mara sin considerar colisiones
        Vector3 idealPosition = pivot - rotation * Vector3.forward * distance;

        // Calcula la direcci�n desde el pivote hacia la posici�n ideal (esta ser� la direcci�n del SphereCast)
        Vector3 direction = (idealPosition - pivot).normalized;

        // Inicialmente, usamos la distancia ideal
        float desiredDistance = distance;

        // Emite un SphereCast desde el pivote en la direcci�n del idealPosition
        RaycastHit hit;
        if (Physics.SphereCast(pivot, collisionRadius, direction, out hit, distance, collisionLayer))
        {
            // Si se detecta un obst�culo, se ajusta la distancia para colocar la c�mara justo antes del mismo
            desiredDistance = Mathf.Clamp(hit.distance, minDistance, distance);
        }

        // Interpolar suavemente la distancia actual hacia la deseada para evitar cambios bruscos
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * smoothSpeed);

        // Calcula la posici�n final de la c�mara usando la distancia ajustada
        Vector3 finalPosition = pivot - rotation * Vector3.forward * currentDistance;

        // Aplica suavizado en la posici�n de la c�mara
        transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * smoothSpeed);

        // La c�mara siempre mira hacia el pivote
        transform.LookAt(pivot);
    }
}