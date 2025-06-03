using System.Collections;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [Header("Drop Physics")]
    public float dropForce = 10f;
    public float arcHeight = 3f;
    public float dropDuration = 1.5f;
    public float spinSpeed = 720f; // Faster spinning during drop (2 rotations per second)

    [Header("Float Animation (After Landing)")]
    public float floatHeight = 0.2f;
    public float floatSpeed = 2f;
    public bool enableGlow = true;
    public Color glowColor = Color.yellow;
    public float glowIntensityMin = 0.8f;
    public float glowIntensityMax = 1.2f;

    [Header("Collection")]
    public string playerTag = "Player";
    public GameObject pickupEffect;
    public float pickupEffectDuration = 2f;

    [Header("Audio")]
    public int dropSoundIndex = 9; // Index for drop sound in AudioManager (different from pickup)
    public int pickupSoundIndex = 1; // Index for pickup sound in AudioManager
    public bool playDropSoundOnLanding = true; // Play drop sound when item lands, not when created

    // Private variables
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;
    protected bool hasLanded = false;
    private bool isFloating = false;
    private Vector3 originalScale;
    protected Vector3 landingPosition;
    private Color originalColor;
    private float timeOffset;
    private bool wasDropped = false; // Track if this item was actually dropped

    // Drop trajectory
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float dropTimer = 0f;
    private bool isDropping = true;

    protected virtual void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();

        // Store original values
        originalScale = transform.localScale;
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Random time offset for glow animation
        timeOffset = Random.Range(0f, 2f * Mathf.PI);

        // Only disable collision if this item will be dropped
        // If it's placed in scene, leave collision enabled
        if (itemCollider != null && !wasDropped)
        {
            itemCollider.enabled = true;
            itemCollider.isTrigger = true;
            hasLanded = true; // Consider pre-placed items as already landed
        }
    }

    public void InitializeDrop(Vector3 dropPosition, Vector3 targetPos)
    {
        startPosition = dropPosition;
        targetPosition = targetPos;
        transform.position = startPosition;
        wasDropped = true; // Mark this item as dropped

        // Disable collision until landed for dropped items
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        // Play drop sound when drop is created (not when it lands)
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(dropSoundIndex);
        }

        // Start the drop animation
        StartCoroutine(DropAnimation());
    }

    IEnumerator DropAnimation()
    {
        dropTimer = 0f;

        while (dropTimer < dropDuration && isDropping)
        {
            float progress = dropTimer / dropDuration;

            // Calculate position along arc
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);

            // Add parabolic arc for Y position
            float arcProgress = 4f * progress * (1f - progress); // Parabolic curve (0 to 1 to 0)
            currentPos.y += arcProgress * arcHeight;

            transform.position = currentPos;

            // Spin the item
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

            dropTimer += Time.deltaTime;
            yield return null;
        }

        // Ensure we land exactly at target
        if (isDropping)
        {
            transform.position = targetPosition;
            hasLanded = true;
            isDropping = false;
            landingPosition = targetPosition;

            // Reset rotation to upright position when landing
            transform.rotation = Quaternion.identity;

            // Drop sound is now played in InitializeDrop instead of here

            // Enable collision for pickup
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
                itemCollider.isTrigger = true;
            }

            // Start floating animation (virtual method that can be overridden)
            StartFloating();
        }
    }

    IEnumerator FloatAnimation()
    {
        isFloating = true;
        float time = timeOffset;

        while (isFloating)
        {
            // Bobbing movement
            float bobOffset = Mathf.Sin(time * floatSpeed) * floatHeight;
            Vector3 newPos = landingPosition;
            newPos.y += bobOffset;
            transform.position = newPos;

            // Glow effect
            if (enableGlow && spriteRenderer != null)
            {
                float glowPulse = (Mathf.Sin(time * floatSpeed * 1.5f) + 1f) * 0.5f;
                float intensity = Mathf.Lerp(glowIntensityMin, glowIntensityMax, glowPulse);
                Color glowedColor = originalColor * glowColor * intensity;
                glowedColor.a = originalColor.a;
                spriteRenderer.color = glowedColor;
            }

            time += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasLanded && other.CompareTag(playerTag))
        {
            CollectItem(other.gameObject);
        }
    }

    void CollectItem(GameObject player)
    {
        // Stop floating
        isFloating = false;

        // Play pickup sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(pickupSoundIndex);
        }

        // Spawn pickup effect
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, pickupEffectDuration);
        }

        // Apply item effect (override this method in derived classes)
        OnItemCollected(player);

        // Destroy the item
        Destroy(gameObject);
    }

    // Virtual method for item-specific effects
    protected virtual void OnItemCollected(GameObject player)
    {
        Debug.Log($"Item {gameObject.name} collected by {player.name}!");

        // Example: Add to inventory, grant experience, etc.
        // This should be overridden by specific item types
    }

    // Static method to easily create drops
    public static GameObject CreateDrop(
        GameObject itemPrefab,
        Vector3 dropPosition,
        Vector3 targetPosition
    )
    {
        GameObject droppedItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);
        ItemDrop itemDrop = droppedItem.GetComponent<ItemDrop>();

        if (itemDrop != null)
        {
            itemDrop.InitializeDrop(dropPosition, targetPosition);
        }
        else
        {
            Debug.LogWarning("ItemDrop component not found on instantiated item!");
        }

        return droppedItem;
    }

    // Virtual method that can be overridden by derived classes
    protected virtual void StartFloating()
    {
        StartCoroutine(FloatAnimation());
    }

    void OnDestroy()
    {
        // Stop all coroutines when destroyed
        StopAllCoroutines();
    }
}
