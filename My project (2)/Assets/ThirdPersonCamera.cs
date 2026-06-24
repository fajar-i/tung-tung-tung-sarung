using UnityEngine;
// Perintah using harus berkumpul di paling atas seperti ini
using UnityEngine.InputSystem; 

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;          
    public float distance = 4.0f;     
    public float heightOffset = 1.5f; 

    [Header("Mouse Settings")]
    public float sensitivity = 0.15f;  

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        currentX += mouseDelta.x * sensitivity;
        currentY -= mouseDelta.y * sensitivity;

        currentY = Mathf.Clamp(currentY, -20f, 60f);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        Vector3 targetPosition = target.position + Vector3.up * heightOffset;
        Vector3 cameraDirection = new Vector3(0, 0, -distance);

        transform.position = targetPosition + rotation * cameraDirection;
        transform.LookAt(targetPosition);
    }
}