using System.Collections;
using UnityEngine;

public class PotionAnimation : MonoBehaviour
{
    [Header("Bobbing Movement")]
    public bool enableBobbing = true;
    public float bobbingHeight = 0.05f; // Amplitude of vertical movement
    public float bobbingPeriod = 2.5f; // Period in seconds (1.0 = one cycle per second)

    [Header("Liquid Glow Effect")]
    public bool enableGlowPulse = true;
    public float pulseSpeed = 2f; // How fast to pulse (slower = more gentle)
    public Color glowTint = new Color(1f, 0.9f, 0.9f, 1f); // Subtle tint
    public float glowIntensityMin = 0.8f; // Minimum glow intensity
    public float glowIntensityMax = 1.3f; // Maximum glow intensity

    [Header("Gentle Rotation")]
    public bool enableRotation = false; // Optional gentle rotation
    public float rotationSpeed = 5f; // Very slow rotation

    // Private variables
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine glowCoroutine;
    private float timeOffset;

    void Start()
    {
        // Store starting position
        startPosition = transform.position;

        // Get sprite renderer for visual effects
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Add random time offset to prevent synchronization
        timeOffset = Random.Range(0f, 2f * Mathf.PI);

        // Start glow effect if enabled
        if (enableGlowPulse)
        {
            StartGlowEffect();
        }
    }

    void Update()
    {
        // Vertical bobbing movement
        if (enableBobbing && bobbingPeriod > 0f)
        {
            // Calculate speed from period: 2π / period
            float bobbingSpeed = (2f * Mathf.PI) / bobbingPeriod;
            float newY =
                startPosition.y
                + Mathf.Sin((Time.time * bobbingSpeed) + timeOffset) * bobbingHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }

        // Optional rotation
        if (enableRotation)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private void StartGlowEffect()
    {
        if (spriteRenderer != null && glowCoroutine == null)
        {
            glowCoroutine = StartCoroutine(GlowEffect());
        }
    }

    private void StopGlowEffect()
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            // Return to original color
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator GlowEffect()
    {
        float time = timeOffset; // Start with random offset

        while (enableGlowPulse)
        {
            if (spriteRenderer != null)
            {
                // Create a smooth pulse using sine wave
                float pulse = (Mathf.Sin(time * pulseSpeed) + 1f) * 0.5f; // Converts -1,1 to 0,1

                // Calculate glow intensity
                float glowIntensity = Mathf.Lerp(glowIntensityMin, glowIntensityMax, pulse);

                // Apply glow by modifying the color
                Color glowColor = originalColor * glowTint * glowIntensity;
                glowColor.a = originalColor.a; // Maintain original alpha
                spriteRenderer.color = glowColor;
            }

            time += Time.deltaTime;
            yield return null; // Wait one frame
        }
    }

    // Public method to customize the potion type with different colors
    public void SetPotionType(PotionType type)
    {
        switch (type)
        {
            case PotionType.Health:
                glowTint = new Color(1f, 0.7f, 0.7f, 1f); // Reddish glow
                break;
            case PotionType.Mana:
                glowTint = new Color(0.7f, 0.7f, 1f, 1f); // Bluish glow
                break;
            case PotionType.Poison:
                glowTint = new Color(0.7f, 1f, 0.7f, 1f); // Greenish glow
                break;
            case PotionType.Magic:
                glowTint = new Color(1f, 0.9f, 0.7f, 1f); // Golden glow
                break;
            default:
                glowTint = new Color(1f, 0.9f, 0.9f, 1f); // Default subtle tint
                break;
        }
    }

    void OnDestroy()
    {
        // Stop glow effect and reset sprite color when destroyed
        StopGlowEffect();
    }

    void OnDisable()
    {
        // Stop glow effect when disabled
        StopGlowEffect();
    }

    void OnEnable()
    {
        // Restart glow effect when enabled
        if (enableGlowPulse && spriteRenderer != null)
        {
            StartGlowEffect();
        }
    }
}

public enum PotionType
{
    Health,
    Mana,
    Poison,
    Magic,
}
