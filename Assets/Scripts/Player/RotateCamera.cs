using UnityEngine;
using UnityEngine.Rendering;

public class RotateCamera : MonoBehaviour
{
    public float rotationSpeed = 1000.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get input on the X Axis
        float mouseInput = Input.GetAxis("Mouse X");

        //Rotate Camera 
        transform.Rotate(Vector3.up, mouseInput * rotationSpeed * Time.deltaTime);
    }

}
