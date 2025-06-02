# Scalable Potion System

This new potion system replaces the old hardcoded potion handling in `PlayerHealth.cs` with a modular, scalable approach that supports multiple potion types and effects.

## System Overview

The potion system consists of four main components:

1. **PotionData** - ScriptableObject that defines potion properties
2. **PotionPickup** - Component for individual potion pickups in the world
3. **PotionManager** - Handles potion effects and player stat management
4. **PotionAnimation** - Handles visual effects and animations (updated)

## Setup Instructions

### 1. Add PotionManager to Player
- Add the `PotionManager` component to your Player GameObject
- The component will automatically find `PlayerHealth` and `HealthBar` components
- Configure base stats (mana, speed, strength) in the inspector

### 2. Create Potion Data Assets
- Right-click in Project window → Create → Potions → Potion Data
- Configure each potion type with:
  - **Potion Name**: Display name
  - **Potion Type**: Health, Mana, Immunity, Speed, Strength, Shield, Experience
  - **Effect Value**: Amount (health points, mana points, percentage for speed, etc.)
  - **Effect Duration**: How long temporary effects last (0 for instant effects)
  - **Glow Tint**: Color for the potion's glow effect
  - **Pickup Effect**: Particle effect prefab to spawn when picked up
  - **Audio Clip Index**: Index for AudioManager sound effect

### 3. Set Up Potion Pickups
- Create a GameObject with:
  - `SpriteRenderer` (for the potion sprite)
  - `Collider2D` with `Is Trigger` enabled
  - `PotionPickup` component
  - `PotionAnimation` component (optional, for visual effects)
- Assign the appropriate `PotionData` asset to the `PotionPickup` component

### 4. Update Existing Potions
- Remove old "Health Potion" and "Bonus Bottle" tags
- Replace with the new `PotionPickup` system
- Update prefabs to use the new components

## Potion Types and Effects

### Health Potion
- **Type**: `PotionType.Health`
- **Effect**: Instantly restores health points
- **Value**: Amount of health to restore
- **Duration**: 0 (instant)

### Mana Potion
- **Type**: `PotionType.Mana`
- **Effect**: Instantly restores mana points
- **Value**: Amount of mana to restore
- **Duration**: 0 (instant)

### Immunity Potion
- **Type**: `PotionType.Immunity`
- **Effect**: Extends player's immortality time
- **Value**: Not used
- **Duration**: Seconds of immunity

### Speed Potion
- **Type**: `PotionType.Speed`
- **Effect**: Temporarily increases movement speed
- **Value**: Percentage increase (50 = 50% faster)
- **Duration**: Seconds the effect lasts

### Strength Potion
- **Type**: `PotionType.Strength`
- **Effect**: Temporarily increases damage/strength
- **Value**: Additional strength points
- **Duration**: Seconds the effect lasts

### Shield Potion
- **Type**: `PotionType.Shield`
- **Effect**: Provides temporary damage absorption
- **Value**: Shield strength (damage points it can absorb)
- **Duration**: Seconds the shield lasts

### Experience Potion
- **Type**: `PotionType.Experience`
- **Effect**: Grants experience points
- **Value**: Amount of XP to grant
- **Duration**: 0 (instant)

## Integration with Other Systems

### Player Movement
To integrate speed potions with your player movement:
```csharp
// In your Player script, subscribe to speed changes
void Start()
{
    PotionManager potionManager = GetComponent<PotionManager>();
    if (potionManager != null)
    {
        potionManager.OnSpeedChanged += UpdateMovementSpeed;
    }
}

void UpdateMovementSpeed(float newSpeed)
{
    // Update your movement speed variable
    moveSpeed = newSpeed;
}
```

### Combat System
To integrate strength potions with your combat:
```csharp
// In your combat script
PotionManager potionManager = GetComponent<PotionManager>();
int finalDamage = baseDamage;
if (potionManager != null)
{
    finalDamage += potionManager.currentStrength - potionManager.baseStrength;
}
```

### UI Integration
Subscribe to events for UI updates:
```csharp
// For mana bar updates
potionManager.OnManaChanged += UpdateManaUI;

void UpdateManaUI(int currentMana, int maxMana)
{
    manaBar.SetMana(currentMana, maxMana);
}
```

## Adding New Potion Types

1. Add new type to `PotionType` enum in `PotionData.cs`
2. Add case in `PotionManager.ApplyPotionEffect()`
3. Add glow color in `PotionAnimation.SetPotionType()`
4. Implement the effect logic in `PotionManager`

## Migration from Old System

The old potion handling in `PlayerHealth.cs` has been removed:
- ❌ `OnTriggerEnter2D` potion detection
- ❌ `healthPotionIncrement` field
- ❌ `potionPickupEffect` field
- ❌ Hardcoded "Health Potion" and "Bonus Bottle" tags

Replace with:
- ✅ `PotionPickup` components on potion GameObjects
- ✅ `PotionData` ScriptableObject assets
- ✅ `PotionManager` on Player GameObject
- ✅ Configurable effects and visual settings

## Benefits of New System

- **Scalable**: Easy to add new potion types
- **Modular**: Each potion type is self-contained
- **Configurable**: All settings in ScriptableObject assets
- **Reusable**: Same components work for all potion types
- **Maintainable**: Clear separation of concerns
- **Extensible**: Easy to add new effects and features 