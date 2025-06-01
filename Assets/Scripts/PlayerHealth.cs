using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth,
        maxHealth,
        damageAmount;
    public HealthBar healthBar;
    public float immortalTime = 0f;
    private float immortalCounter = 0f;

    // public GameObject immortalEffect; // Removed - using subtle transparency instead
    public int healthPotionIncrement = 1;

    // Visual components for immortality effect
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isImmortal = false;
    private Coroutine flashCoroutine;

    // MapleStory-style flash settings
    [Header("Flash Effect Settings")]
    public float pulseSpeed = 3f; // How fast to pulse (slower = more gentle)
    public Color tintColor = Color.white; // Pure white tint

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

                // Very gentle alpha pulse between 85% and 100% opacity
                float alpha = 0.80f + (pulse * 0.15f); // Alpha from 0.85 to 1.0
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
            // Use custom damage if provided, otherwise use default damageAmount
            int actualDamage = customDamage > 0 ? customDamage : damageAmount;

            currentHealth -= actualDamage;
            healthBar.SetHealth(currentHealth);
            if (currentHealth <= 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                immortalCounter = immortalTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Bonus Bottle")
        {
            AudioManager.instance.PlaySFX(2);
            immortalCounter = immortalTime + 2;
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Health Potion")
        {
            AudioManager.instance.PlaySFX(1);
            if (currentHealth < maxHealth)
            {
                currentHealth += healthPotionIncrement;
                healthBar.SetHealth(currentHealth);
            }
            Destroy(collision.gameObject);
        }
    }
}
