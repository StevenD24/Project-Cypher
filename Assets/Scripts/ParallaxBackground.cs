using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    public Transform[] layers; // Assign in inspector

    [Tooltip("Parallax multipliers - 0 = no movement, 1 = moves with camera, 0.5 = half speed")]
    public float[] parallaxScales = { 0.1f, 0.3f, 0.5f, 0.7f, 0.9f }; // Default values with clear differences
    public float smoothing = 1f; // How smooth the parallax is

    private Transform cam; // Reference to the main camera's transform
    private Vector3 previousCamPos; // Position of the camera in the previous frame

    void Awake()
    {
        cam = Camera.main.transform;
    }

    void Start()
    {
        previousCamPos = cam.position;

        // Validation and debugging
        if (layers.Length == 0)
        {
            Debug.LogWarning(
                "Parallax: No layers assigned! Please assign background layers in the inspector."
            );
            return;
        }

        if (parallaxScales.Length != layers.Length)
        {
            Debug.LogWarning(
                $"Parallax: Number of layers ({layers.Length}) doesn't match number of parallax scales ({parallaxScales.Length}). Resizing parallax scales array."
            );

            // Auto-generate parallax scales if they don't match
            parallaxScales = new float[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                // Generate scales from 0.1 to 0.9 based on layer index
                parallaxScales[i] = 0.1f + (0.8f * i / (layers.Length - 1));
            }
        }

        // Debug output
        Debug.Log("Parallax Setup:");
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] != null)
            {
                Debug.Log($"Layer {i}: {layers[i].name} - Parallax Scale: {parallaxScales[i]}");
            }
            else
            {
                Debug.LogWarning($"Layer {i} is null! Please assign a layer in the inspector.");
            }
        }
    }

    void Update()
    {
        if (layers.Length == 0 || cam == null)
            return;

        // Calculate camera movement
        float cameraMovement = cam.position.x - previousCamPos.x;

        // Apply parallax to each layer
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] == null)
                continue;

            // Calculate parallax offset
            float parallaxOffset = cameraMovement * (1 - parallaxScales[i]);

            // Apply the offset
            Vector3 currentPos = layers[i].position;
            float targetX = currentPos.x + parallaxOffset;
            Vector3 targetPos = new Vector3(targetX, currentPos.y, currentPos.z);

            // Apply smoothing
            layers[i].position = Vector3.Lerp(currentPos, targetPos, smoothing * Time.deltaTime);

            // Debug output (uncomment to see real-time values)
            // Debug.Log($"Layer {i}: Camera moved {cameraMovement:F3}, Parallax offset: {parallaxOffset:F3}");
        }

        previousCamPos = cam.position;
    }
}
