using UnityEngine;

public class PotionParticleEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    public GameObject particlePrefab; // Optional particle system prefab
    public bool createSimpleParticles = true; // Create simple built-in particles
    public int maxParticles = 8; // Maximum number of particles
    public float particleLifetime = 2f; // How long particles live
    public float emissionRate = 0.5f; // Particles per second

    [Header("Particle Movement")]
    public float particleSpeed = 0.3f; // How fast particles move
    public float particleRange = 0.5f; // How far particles travel from center
    public Vector2 particleScaleRange = new Vector2(0.1f, 0.3f); // Scale variation

    [Header("Particle Appearance")]
    public Color particleStartColor = new Color(1f, 1f, 1f, 0.8f);
    public Color particleEndColor = new Color(1f, 1f, 1f, 0f);
    public Sprite particleSprite; // Optional custom sprite for particles

    // Private variables
    private float timeSinceLastEmission = 0f;
    private Transform[] activeParticles;
    private float[] particleTimer;
    private Vector3[] particleDirection;
    private float[] particleScale;
    private SpriteRenderer[] particleRenderers;
    private int particleIndex = 0;

    void Start()
    {
        if (createSimpleParticles)
        {
            InitializeSimpleParticles();
        }
    }

    void InitializeSimpleParticles()
    {
        activeParticles = new Transform[maxParticles];
        particleTimer = new float[maxParticles];
        particleDirection = new Vector3[maxParticles];
        particleScale = new float[maxParticles];
        particleRenderers = new SpriteRenderer[maxParticles];

        // Pre-create particle GameObjects
        for (int i = 0; i < maxParticles; i++)
        {
            GameObject particle = new GameObject("PotionParticle_" + i);
            particle.transform.SetParent(transform);

            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = particleSprite != null ? particleSprite : CreateSimpleCircleSprite();
            sr.color = particleStartColor;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 10; // Render above potion

            activeParticles[i] = particle.transform;
            particleRenderers[i] = sr;
            particleTimer[i] = -1f; // Inactive

            particle.SetActive(false);
        }
    }

    void Update()
    {
        if (createSimpleParticles)
        {
            UpdateSimpleParticles();
        }
    }

    void UpdateSimpleParticles()
    {
        timeSinceLastEmission += Time.deltaTime;

        // Emit new particle
        if (timeSinceLastEmission >= (1f / emissionRate))
        {
            EmitParticle();
            timeSinceLastEmission = 0f;
        }

        // Update existing particles
        for (int i = 0; i < maxParticles; i++)
        {
            if (particleTimer[i] > 0f)
            {
                UpdateParticle(i);
            }
        }
    }

    void EmitParticle()
    {
        // Find inactive particle
        int index = FindInactiveParticle();
        if (index == -1)
            return; // No available particles

        // Initialize particle
        particleTimer[index] = particleLifetime;

        // Random direction in circle around potion
        float angle = Random.Range(0f, 2f * Mathf.PI);
        particleDirection[index] = new Vector3(
            Mathf.Cos(angle) * particleSpeed,
            Mathf.Sin(angle) * particleSpeed + 0.1f, // Slight upward bias
            0f
        );

        // Random scale
        particleScale[index] = Random.Range(particleScaleRange.x, particleScaleRange.y);

        // Position near potion center with slight random offset
        Vector3 startPos =
            transform.position
            + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0f);

        activeParticles[index].position = startPos;
        activeParticles[index].localScale = Vector3.one * particleScale[index];
        activeParticles[index].gameObject.SetActive(true);

        // Set initial color
        particleRenderers[index].color = particleStartColor;
    }

    void UpdateParticle(int index)
    {
        particleTimer[index] -= Time.deltaTime;

        if (particleTimer[index] <= 0f)
        {
            // Deactivate particle
            activeParticles[index].gameObject.SetActive(false);
            return;
        }

        // Move particle
        activeParticles[index].position += particleDirection[index] * Time.deltaTime;

        // Fade particle over lifetime
        float lifeRatio = particleTimer[index] / particleLifetime;
        Color currentColor = Color.Lerp(particleEndColor, particleStartColor, lifeRatio);
        particleRenderers[index].color = currentColor;

        // Optional: Scale particle over lifetime (subtle breathing effect)
        float scaleMultiplier = 0.8f + 0.4f * Mathf.Sin(Time.time * 3f + index); // Slight variation
        activeParticles[index].localScale = Vector3.one * particleScale[index] * scaleMultiplier;

        // Optional: Slow down particle over time
        particleDirection[index] *= 0.98f;
    }

    int FindInactiveParticle()
    {
        for (int i = 0; i < maxParticles; i++)
        {
            if (particleTimer[i] <= 0f)
            {
                return i;
            }
        }
        return -1; // No inactive particle found
    }

    Sprite CreateSimpleCircleSprite()
    {
        // Create a simple white circle texture
        int size = 16;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 1f - Mathf.Clamp01(distance / radius);
                alpha = Mathf.SmoothStep(0f, 1f, alpha); // Smooth edges

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // Public method to set particle color based on potion type
    public void SetParticleColor(PotionType type)
    {
        switch (type)
        {
            case PotionType.Health:
                particleStartColor = new Color(1f, 0.3f, 0.3f, 0.8f); // Red sparkles
                particleEndColor = new Color(1f, 0.3f, 0.3f, 0f);
                break;
            case PotionType.Mana:
                particleStartColor = new Color(0.3f, 0.3f, 1f, 0.8f); // Blue sparkles
                particleEndColor = new Color(0.3f, 0.3f, 1f, 0f);
                break;
            case PotionType.Poison:
                particleStartColor = new Color(0.3f, 1f, 0.3f, 0.8f); // Green sparkles
                particleEndColor = new Color(0.3f, 1f, 0.3f, 0f);
                break;
            case PotionType.Magic:
                particleStartColor = new Color(1f, 0.8f, 0.3f, 0.8f); // Golden sparkles
                particleEndColor = new Color(1f, 0.8f, 0.3f, 0f);
                break;
            default:
                particleStartColor = new Color(1f, 1f, 1f, 0.8f); // White sparkles
                particleEndColor = new Color(1f, 1f, 1f, 0f);
                break;
        }

        // Update existing particle colors if they exist
        if (particleRenderers != null)
        {
            for (int i = 0; i < maxParticles; i++)
            {
                if (particleTimer[i] > 0f)
                {
                    float lifeRatio = particleTimer[i] / particleLifetime;
                    Color currentColor = Color.Lerp(
                        particleEndColor,
                        particleStartColor,
                        lifeRatio
                    );
                    particleRenderers[i].color = currentColor;
                }
            }
        }
    }

    void OnDestroy()
    {
        // Clean up particle GameObjects
        if (activeParticles != null)
        {
            for (int i = 0; i < activeParticles.Length; i++)
            {
                if (activeParticles[i] != null)
                {
                    DestroyImmediate(activeParticles[i].gameObject);
                }
            }
        }
    }
}
