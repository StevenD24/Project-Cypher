using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionManager : MonoBehaviour
{
    [Header("Component References")]
    public PlayerHealth playerHealth;
    public HealthBar healthBar;

    [Header("Player Stats")]
    public int currentMana = 100;
    public int maxMana = 100;
    public float currentSpeed = 5f;
    public float baseSpeed = 5f;
    public int currentStrength = 1;
    public int baseStrength = 1;

    [Header("Active Effects")]
    public List<ActivePotionEffect> activeEffects = new List<ActivePotionEffect>();

    // Events for UI updates
    public System.Action<int, int> OnManaChanged; // current, max
    public System.Action<float> OnSpeedChanged;
    public System.Action<int> OnStrengthChanged;

    private void Start()
    {
        // Auto-find components if not assigned
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
        if (healthBar == null)
            healthBar = FindObjectOfType<HealthBar>();

        // Initialize stats
        currentMana = maxMana;
        currentSpeed = baseSpeed;
        currentStrength = baseStrength;
    }

    private void Update()
    {
        UpdateActiveEffects();
    }

    public void ApplyPotionEffect(PotionData potionData)
    {
        if (potionData == null)
            return;

        switch (potionData.potionType)
        {
            case PotionType.Health:
                ApplyHealthPotion(potionData);
                break;
            case PotionType.Mana:
                ApplyManaPotion(potionData);
                break;
            case PotionType.Immunity:
                ApplyImmunityPotion(potionData);
                break;
            case PotionType.Speed:
                ApplySpeedPotion(potionData);
                break;
            case PotionType.Strength:
                ApplyStrengthPotion(potionData);
                break;
            case PotionType.Shield:
                ApplyShieldPotion(potionData);
                break;
            case PotionType.Experience:
                ApplyExperiencePotion(potionData);
                break;
            default:
                Debug.LogWarning($"Potion type {potionData.potionType} not implemented yet!");
                break;
        }
    }

    private void ApplyHealthPotion(PotionData potionData)
    {
        if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth)
        {
            playerHealth.currentHealth = Mathf.Min(
                playerHealth.currentHealth + potionData.effectValue,
                playerHealth.maxHealth
            );

            if (healthBar != null)
                healthBar.SetHealth(playerHealth.currentHealth);
        }
    }

    private void ApplyManaPotion(PotionData potionData)
    {
        currentMana = Mathf.Min(currentMana + potionData.effectValue, maxMana);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    private void ApplyImmunityPotion(PotionData potionData)
    {
        if (playerHealth != null)
        {
            // Add extra immunity time
            playerHealth.immortalCounter = Mathf.Max(
                playerHealth.immortalCounter,
                potionData.effectDuration
            );
        }
    }

    private void ApplySpeedPotion(PotionData potionData)
    {
        // Remove existing speed effect if any
        RemoveEffectOfType(PotionType.Speed);

        // Apply new speed boost
        float speedMultiplier = 1f + (potionData.effectValue / 100f); // effectValue as percentage
        currentSpeed = baseSpeed * speedMultiplier;

        // Add to active effects
        ActivePotionEffect speedEffect = new ActivePotionEffect
        {
            potionType = PotionType.Speed,
            remainingTime = potionData.effectDuration,
            originalValue = baseSpeed,
            modifiedValue = currentSpeed,
        };
        activeEffects.Add(speedEffect);

        OnSpeedChanged?.Invoke(currentSpeed);

        // Apply to player movement (you'll need to modify your Player script to use this)
        Player playerScript = GetComponent<Player>();
        if (playerScript != null)
        {
            // You'll need to add a method to Player script to update speed
            // playerScript.UpdateSpeed(currentSpeed);
        }
    }

    private void ApplyStrengthPotion(PotionData potionData)
    {
        // Remove existing strength effect if any
        RemoveEffectOfType(PotionType.Strength);

        // Apply strength boost
        currentStrength = baseStrength + potionData.effectValue;

        // Add to active effects
        ActivePotionEffect strengthEffect = new ActivePotionEffect
        {
            potionType = PotionType.Strength,
            remainingTime = potionData.effectDuration,
            originalValue = baseStrength,
            modifiedValue = currentStrength,
        };
        activeEffects.Add(strengthEffect);

        OnStrengthChanged?.Invoke(currentStrength);
    }

    private void ApplyShieldPotion(PotionData potionData)
    {
        // Add temporary shield effect
        ActivePotionEffect shieldEffect = new ActivePotionEffect
        {
            potionType = PotionType.Shield,
            remainingTime = potionData.effectDuration,
            originalValue = 0,
            modifiedValue = potionData.effectValue, // Shield strength
        };
        activeEffects.Add(shieldEffect);

        // Visual effect could be added here
    }

    private void ApplyExperiencePotion(PotionData potionData)
    {
        // Add experience points - you can integrate this with your XP system
        Debug.Log($"Gained {potionData.effectValue} experience points!");
        // Example: GameManager.instance.AddExperience(potionData.effectValue);
    }

    private void UpdateActiveEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].remainingTime -= Time.deltaTime;

            if (activeEffects[i].remainingTime <= 0)
            {
                // Effect expired, remove it
                RemoveEffect(i);
            }
        }
    }

    private void RemoveEffect(int index)
    {
        if (index < 0 || index >= activeEffects.Count)
            return;

        ActivePotionEffect effect = activeEffects[index];

        // Restore original values
        switch (effect.potionType)
        {
            case PotionType.Speed:
                currentSpeed = baseSpeed;
                OnSpeedChanged?.Invoke(currentSpeed);
                break;
            case PotionType.Strength:
                currentStrength = baseStrength;
                OnStrengthChanged?.Invoke(currentStrength);
                break;
        }

        activeEffects.RemoveAt(index);
    }

    private void RemoveEffectOfType(PotionType type)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].potionType == type)
            {
                RemoveEffect(i);
                break; // Only remove the first one found
            }
        }
    }

    // Public methods for other scripts to check effects
    public bool HasActiveEffect(PotionType type)
    {
        return activeEffects.Exists(effect => effect.potionType == type);
    }

    public float GetEffectRemainingTime(PotionType type)
    {
        var effect = activeEffects.Find(e => e.potionType == type);
        return effect != null ? effect.remainingTime : 0f;
    }

    public bool HasShield()
    {
        return HasActiveEffect(PotionType.Shield);
    }

    public int GetShieldStrength()
    {
        var shieldEffect = activeEffects.Find(e => e.potionType == PotionType.Shield);
        return shieldEffect != null ? (int)shieldEffect.modifiedValue : 0;
    }
}

[System.Serializable]
public class ActivePotionEffect
{
    public PotionType potionType;
    public float remainingTime;
    public float originalValue;
    public float modifiedValue;
}
