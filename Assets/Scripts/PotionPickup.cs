using UnityEngine;

public class PotionPickup : MonoBehaviour
{
    [Header("Potion Configuration")]
    public PotionData potionData;

    private PotionAnimation potionAnimation;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        InitializePotion();
    }

    private void InitializePotion()
    {
        if (potionData == null)
        {
            Debug.LogWarning($"PotionPickup on {gameObject.name} has no PotionData assigned!");
            return;
        }

        // Set up sprite
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && potionData.potionSprite != null)
        {
            spriteRenderer.sprite = potionData.potionSprite;
        }

        // Set up animation
        potionAnimation = GetComponent<PotionAnimation>();
        if (potionAnimation != null)
        {
            // Apply custom animation settings if specified
            if (potionData.enableCustomBobbing)
            {
                potionAnimation.bobbingHeight = potionData.bobbingHeight;
                potionAnimation.bobbingPeriod = potionData.bobbingPeriod;
            }

            // Set glow tint
            potionAnimation.glowTint = potionData.glowTint;

            // Set potion type for animation
            potionAnimation.SetPotionType(potionData.potionType);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Get the PotionManager from the player
            PotionManager potionManager = collision.GetComponent<PotionManager>();
            if (potionManager != null)
            {
                // Apply the potion effect
                potionManager.ApplyPotionEffect(potionData);

                // Play audio
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySFX(potionData.audioClipIndex);
                }

                // Instantiate pickup effect
                if (potionData.pickupEffect != null)
                {
                    GameObject effect = Instantiate(
                        potionData.pickupEffect,
                        collision.transform.position,
                        Quaternion.identity
                    );
                    effect.transform.SetParent(collision.transform);
                    Destroy(effect, potionData.effectDuration_Visual);
                }

                // Destroy the potion
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("Player doesn't have a PotionManager component!");
            }
        }
    }

    // Method to set potion data at runtime (useful for spawning different potions)
    public void SetPotionData(PotionData data)
    {
        potionData = data;
        InitializePotion();
    }
}
