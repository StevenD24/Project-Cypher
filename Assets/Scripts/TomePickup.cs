using UnityEngine;

public class TomePickup : ItemDrop
{
    [Header("Tome Properties")]
    public string tomeName = "Ancient Tome";
    public TomeType tomeType = TomeType.Experience;
    public int experienceValue = 100;
    public int goldValue = 50;
    public string abilityUnlocked = "";

    [Header("Visual Effects")]
    public Color tomeGlowColor = Color.cyan;
    public float tomeFloatHeight = 0.3f;
    public ParticleSystem magicParticles;

    [Header("Tome Audio")]
    public int tomeDropSoundIndex = 9; // Different sound for tome drops (more magical)
    public int tomePickupSoundIndex = 1; // Standard pickup sound

    [Header("Enhanced Bobbing (Like Potions)")]
    public bool enableEnhancedBobbing = true;
    public float bobbingHeight = 0.05f; // Amplitude of vertical movement (like potions)
    public float bobbingPeriod = 2.5f; // Period in seconds (like potions)
    public bool enableRotation = true; // Enable gentle rotation by default
    public float rotationSpeed = 45f; // Moderate rotation speed while bobbing

    // Private variables for enhanced bobbing
    private Vector3 startPosition;
    private float timeOffset;
    private bool isUsingEnhancedBobbing = false;

    protected override void Start()
    {
        // Set tome-specific properties
        glowColor = tomeGlowColor;
        floatHeight = tomeFloatHeight;

        // Set tome-specific audio
        dropSoundIndex = tomeDropSoundIndex;
        pickupSoundIndex = tomePickupSoundIndex;

        // Set up enhanced bobbing (override the base float settings)
        if (enableEnhancedBobbing)
        {
            floatHeight = bobbingHeight;
            floatSpeed = (2f * Mathf.PI) / bobbingPeriod; // Convert period to speed
            isUsingEnhancedBobbing = true;
        }

        // Store starting position for bobbing
        startPosition = transform.position;

        // Add random time offset to prevent synchronization (like potions)
        timeOffset = Random.Range(0f, 2f * Mathf.PI);

        // Start magic particles if available
        if (magicParticles != null)
        {
            magicParticles.Play();
        }

        // Call base Start method
        base.Start();

        // If this item wasn't dropped (pre-placed in scene), start enhanced bobbing immediately
        if (enableEnhancedBobbing && hasLanded)
        {
            landingPosition = transform.position;
        }
    }

    void Update()
    {
        // Enhanced bobbing that works like potions (continuous, even when not dropped)
        if (enableEnhancedBobbing && hasLanded && landingPosition != Vector3.zero)
        {
            // Use potion-style bobbing calculation
            if (bobbingPeriod > 0f)
            {
                // Calculate speed from period: 2π / period
                float bobbingSpeed = (2f * Mathf.PI) / bobbingPeriod;
                float newY =
                    landingPosition.y
                    + Mathf.Sin((Time.time * bobbingSpeed) + timeOffset) * bobbingHeight;
                transform.position = new Vector3(landingPosition.x, newY, landingPosition.z);
            }

            // Optional rotation like potions
            if (enableRotation)
            {
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // Override the float animation to use our enhanced version
    protected override void StartFloating()
    {
        if (enableEnhancedBobbing)
        {
            // Store the landing position for enhanced bobbing
            landingPosition = transform.position;
            hasLanded = true;

            // Ensure we start rotation from upright position
            transform.rotation = Quaternion.identity;

            // Don't start the base floating animation, we'll handle it in Update
        }
        else
        {
            // Use the base floating animation
            base.StartFloating();
        }
    }

    protected override void OnItemCollected(GameObject player)
    {
        Debug.Log($"Tome '{tomeName}' collected!");

        // Apply tome-specific effects
        switch (tomeType)
        {
            case TomeType.Experience:
                GrantExperience(player);
                break;
            case TomeType.Gold:
                GrantGold(player);
                break;
            case TomeType.Ability:
                UnlockAbility(player);
                break;
            case TomeType.Mixed:
                GrantExperience(player);
                GrantGold(player);
                break;
        }

        // Show collection message
        ShowCollectionMessage();
    }

    void GrantExperience(GameObject player)
    {
        if (experienceValue > 0)
        {
            Debug.Log($"Granted {experienceValue} experience points!");

            // You can integrate this with your experience system
            // Example: GameManager.instance.AddExperience(experienceValue);
            // Example: PlayerProgression playerProgression = player.GetComponent<PlayerProgression>();
            // if (playerProgression != null) playerProgression.GainExperience(experienceValue);
        }
    }

    void GrantGold(GameObject player)
    {
        if (goldValue > 0)
        {
            Debug.Log($"Granted {goldValue} gold!");

            // You can integrate this with your currency system
            // Example: GameManager.instance.AddGold(goldValue);
            // Example: PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            // if (inventory != null) inventory.AddGold(goldValue);
        }
    }

    void UnlockAbility(GameObject player)
    {
        if (!string.IsNullOrEmpty(abilityUnlocked))
        {
            Debug.Log($"Unlocked ability: {abilityUnlocked}!");

            // You can integrate this with your ability system
            // Example: PlayerAbilities abilities = player.GetComponent<PlayerAbilities>();
            // if (abilities != null) abilities.UnlockAbility(abilityUnlocked);
        }
    }

    void ShowCollectionMessage()
    {
        // You can integrate this with your UI system to show collection messages
        // Example: UIManager.instance.ShowMessage($"Found {tomeName}!");
        Debug.Log($"Collection message: Found {tomeName}!");
    }
}

public enum TomeType
{
    Experience,
    Gold,
    Ability,
    Mixed,
}
