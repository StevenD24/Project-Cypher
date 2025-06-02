using UnityEngine;

public class CoinBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float bobbingSpeed = 1f; // How fast the coin bobs up and down
    public float bobbingHeight = 0.1f; // How high the coin moves up and down
    public float randomOffset = 0f; // Random offset to prevent all coins from syncing

    [Header("Rotation Settings")]
    public bool enableRotation = true;
    public float rotationSpeed = 10f; // Degrees per second

    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        // Store the starting position
        startPosition = transform.position;

        // Add random offset to prevent all coins from moving in sync
        timeOffset = Random.Range(0f, 2f * Mathf.PI);

        // If randomOffset is set, add additional randomization
        if (randomOffset > 0f)
        {
            timeOffset += Random.Range(-randomOffset, randomOffset);
        }
    }

    void Update()
    {
        // Calculate the bobbing motion using sine wave
        float newY =
            startPosition.y + Mathf.Sin((Time.time * bobbingSpeed) + timeOffset) * bobbingHeight;

        // Apply the new position
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Optional rotation for extra visual appeal
        if (enableRotation)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
}
