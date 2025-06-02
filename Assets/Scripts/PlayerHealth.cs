using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth,
        maxHealth,
        damageAmount;
    public HealthBar healthBar;
    public float immortalTime = 0f;
    public float immortalCounter = 0f; // Made public so PotionManager can access it

    // Visual components for immortality effect
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isImmortal = false;
    private Coroutine flashCoroutine;

    // MapleStory-style flash settings
    [Header("Flash Effect Settings")]
    public float pulseSpeed = 3f; // How fast to pulse (slower = more gentle)
    public Color tintColor = Color.white; // Pure white tint

    [Header("Death Effect")]
    public GameObject deathEffectPrefab; // Prefab to spawn when player dies

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(currentHealth);

        // Get sprite renderer for visual effects
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (immortalCounter > 0)
        {
            immortalCounter -= Time.deltaTime;

            // Apply subtle visual effect while immortal
            if (!isImmortal)
            {
                isImmortal = true;
                ApplyImmortalVisual();
            }

            if (immortalCounter <= 0)
            {
                isImmortal = false;
                RemoveImmortalVisual();
            }
        }
    }

    private void ApplyImmortalVisual()
    {
        if (spriteRenderer != null && flashCoroutine == null)
        {
            flashCoroutine = StartCoroutine(FlashEffect());
        }
    }

    private void RemoveImmortalVisual()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            // Return to original color
            spriteRenderer.color = originalColor;
            spriteRenderer.enabled = true; // Make sure sprite is enabled
        }
    }

    private IEnumerator FlashEffect()
    {
        float time = 0f;

        while (isImmortal)
        {
            if (spriteRenderer != null)
            {
                // Create a smooth pulse using sine wave
                float pulse = (Mathf.Sin(time * pulseSpeed) + 1f) * 0.5f; // Converts -1,1 to 0,1

                // Very gentle alpha pulse between 70% and 100% opacity
                float alpha = 0.70f + (pulse * 0.30f); // Alpha from 0.70 to 1.0
                Color currentColor = originalColor;
                currentColor.a = alpha;
                spriteRenderer.color = currentColor;
            }

            time += Time.deltaTime;
            yield return null; // Wait one frame
        }
    }

    public void DealDamage(int customDamage = -1)
    {
        if (immortalCounter <= 0)
        {
            // Check for shield protection from PotionManager
            PotionManager potionManager = GetComponent<PotionManager>();
            if (potionManager != null && potionManager.HasShield())
            {
                // Shield absorbs damage
                int shieldStrength = potionManager.GetShieldStrength();
                int actualDamage = customDamage > 0 ? customDamage : damageAmount;

                if (shieldStrength >= actualDamage)
                {
                    // Shield completely absorbs damage
                    Debug.Log("Shield absorbed damage!");
                    return;
                }
                else
                {
                    // Shield partially absorbs damage
                    actualDamage -= shieldStrength;
                    Debug.Log(
                        $"Shield absorbed {shieldStrength} damage, {actualDamage} damage taken!"
                    );
                }
            }

            // Use custom damage if provided, otherwise use default damageAmount
            int finalDamage = customDamage > 0 ? customDamage : damageAmount;

            currentHealth -= finalDamage;
            healthBar.SetHealth(currentHealth);
            if (currentHealth <= 0)
            {
                // Instantiate death effect if available
                if (deathEffectPrefab != null)
                {
                    Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                    Debug.Log("Player death effect spawned!");
                }

                gameObject.SetActive(false);
            }
            else
            {
                immortalCounter = immortalTime;
            }
        }
    }
}
