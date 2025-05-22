using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform[] layers; // Assign in inspector
    public float[] parallaxScales; // Proportion of camera movement to move the backgrounds by
    public float smoothing = 1f;   // How smooth the parallax is

    private Transform cam;         // Reference to the main camera's transform
    private Vector3 previousCamPos; // Position of the camera in the previous frame

    void Awake()
    {
        cam = Camera.main.transform;
    }

    void Start()
    {
        previousCamPos = cam.position;
    }

    void Update()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            float parallax = (previousCamPos.x - cam.position.x) * parallaxScales[i];
            float targetX = layers[i].position.x + parallax;
            Vector3 targetPos = new Vector3(targetX, layers[i].position.y, layers[i].position.z);
            layers[i].position = Vector3.Lerp(layers[i].position, targetPos, smoothing * Time.deltaTime);
        }
        previousCamPos = cam.position;
    }
}