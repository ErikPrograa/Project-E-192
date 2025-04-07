using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlatformerCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -6);
    public float smoothSpeed = 0.1f;
    public float sensitivity = 120f;

    private float rotationY;
    private float rotationX;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotationY = angles.y;
        rotationX = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {

        /* rotationY += Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
         rotationX += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
         rotationY = Mathf.Clamp(rotationY, -60f, 60f);*/
    }
    private void FixedUpdate()
    {
        HandleCameraFollowPlayer();

    }
    private void LateUpdate()
    {
        HandleCameraRotation();
    }

    void HandleCameraFollowPlayer()
    {
        if(InputManager.Instance.movementInput.y !=0)
        {
            Debug.Log("Player is moving vertically");
            transform.position = Vector3.Lerp(transform.position, target.position + offset,Time.deltaTime);
        }
        transform.position = Vector3.Lerp(transform.position,new Vector3(transform.position.x,target.position.y +offset.y, target.position.z + offset.z), 2f* Time.deltaTime);
    }

    void HandleCameraRotation()
    {
        /*Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 desiredPosition = target.position + rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);*/
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
