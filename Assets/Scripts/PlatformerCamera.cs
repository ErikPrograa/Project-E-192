using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlatformerCamera : MonoBehaviour
{
    [Header("Referencia al Objetivo")]
    public Transform target; // Objeto a seguir (por ejemplo, el jugador)

    [Header("Par�metros de la C�mara")]
    public float distance = 5.0f;      // Distancia desde el objetivo
    public float height = 2.0f;        // Altura del punto de pivote sobre el objetivo
    public float rotationSpeed = 5.0f; // Velocidad de giro (input del mouse)
    public float smoothSpeed = 10f;    // Suavizado de movimientos

    [Header("Control de �ngulo Vertical")]
    public float minPitch = -40f; // �ngulo m�nimo (mirar hacia abajo)
    public float maxPitch = 85f;  // �ngulo m�ximo (mirar hacia arriba)

    private float currentAngle = 0f;  // �ngulo horizontal (yaw)
    private float currentPitch = 10f; // �ngulo vertical inicial (pitch)
    PlayerController playerController;
    float rotationX;
    float rotationY;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotationY = angles.y;
        rotationX = angles.x;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        playerController = target.GetComponentInParent<PlayerController>(); 
    }
    private void Update()
    {

        /*rotationY -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        rotationX += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -60f, 60f);*/
        
    }
    private void FixedUpdate()
    {
        

    }
    private void LateUpdate()
    {
        // Actualiza el �ngulo horizontal seg�n el input del mouse
        currentAngle += Input.GetAxis("Mouse X") * rotationSpeed;

        // Actualiza el �ngulo vertical (invertir para una sensaci�n natural)
        currentPitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // Crea la rotaci�n compuesta a partir del pitch y yaw
        Quaternion rotation = Quaternion.Euler(currentPitch, currentAngle, 0);

        // Define el punto de pivote (puede ser el centro del jugador con un offset vertical)
        Vector3 pivot = target.position + Vector3.up * height;

        // Calcula la posici�n deseada: partiendo del pivote se retrocede a la distancia deseada en la direcci�n opuesta a la "forward"
        Vector3 desiredPosition = pivot - rotation * Vector3.forward * distance;

        // Suaviza la transici�n a la posici�n deseada
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.fixedDeltaTime * smoothSpeed);

        // La c�mara mira siempre hacia el pivote
        transform.LookAt(pivot);
    }

    void HandleCameraFollowPlayer()
    {
    }

    void HandleCameraRotation()
    {
        
        
    }
}
