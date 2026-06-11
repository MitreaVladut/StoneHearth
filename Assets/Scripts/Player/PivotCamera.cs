using UnityEngine;

public class PivotCamera : MonoBehaviour
{

    public Transform target; // The Player (parent)
    public Transform cameraTransform; // The Camera (child)
    public float rotationSpeed = 2f; // Mouse sensitivity
    public float rotationSmoothSpeed = 0.1f; // Smoothing factor for rotation
    public float distance = 5f; // Distance from player
    public float minPitch = -30f; // Min vertical angle
    public float maxPitch = 60f; // Max vertical angle
    public float heightOffset = 1f; // Height above player's feet

    private float yaw = 0f; // Horizontal rotation
    private float pitch = 0f; // Vertical rotation

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Lock cursor for better control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize yaw and pitch based on current rotation
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        yaw = eulerAngles.y;
        pitch = eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null || cameraTransform == null) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Update yaw and pitch
        yaw += mouseX;
        pitch -= mouseY; // Invert for natural feel
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Smoothly interpolate the rotation
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed);

        // Position pivot at player's position (with height offset)
        transform.position = target.position + Vector3.up * heightOffset;

        // Position the camera at a fixed distance behind the pivot
        cameraTransform.position = transform.position - transform.forward * distance;

        // Ensure camera looks at the pivot (player's position with offset)
        cameraTransform.LookAt(transform.position);
    }
}

