using UnityEngine;

public class BobAndRotate : MonoBehaviour
{
    [Header("Bob Settings")]
    public float bobAmplitude = 0.5f; // The height of the bobbing motion
    public float bobFrequency = 1f;  // How fast the object bobs

    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(0, 50, 0); // Rotation speed in degrees per second

    public Vector3 startPosition;

    private void Start()
    {
        // Save the starting position of the object
        startPosition = transform.position;
    }

    private void Update()
    {
        // Bobbing motion (sinusoidal)
        float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = startPosition + new Vector3(0, bobOffset, 0);

        // Rotation
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
