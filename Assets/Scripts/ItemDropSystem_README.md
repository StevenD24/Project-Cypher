# MapleStory-Style Item Drop System

This system creates item drops that spin while falling in an arc pattern, then float and glow like in MapleStory.

## Components Overview

1. **ItemDrop.cs** - Base class for all droppable items
2. **TomePickup.cs** - Specific implementation for tome items  
3. **BossDropSystem.cs** - Manages what drops when bosses die
4. **Modified RobotBoss.cs** - Triggers drops on death

## Quick Setup Guide

### Step 1: Create Your Tome Prefab

1. **Create a new GameObject** for your tome
2. **Add these components:**
   - `SpriteRenderer` (assign your TomeBlue_0 sprite)
   - `Collider2D` (set as Trigger)
   - `TomePickup` script
   - `Rigidbody2D` (optional, for physics)

3. **Configure TomePickup component:**
   ```
   Tome Properties:
   - Tome Name: "Ancient Blue Tome"
   - Tome Type: Experience (or Gold/Ability/Mixed)
   - Experience Value: 100
   - Gold Value: 50
   
   Drop Physics:
   - Drop Force: 10
   - Arc Height: 3
   - Drop Duration: 1.5
   - Spin Speed: 360
   
   Float Animation:
   - Float Height: 0.3
   - Float Speed: 2
   - Enable Glow: ✓
   - Glow Color: Cyan (0, 1, 1, 1)
   
   Audio:
   - Drop Sound Index: 1
   - Pickup Sound Index: 1
   ```

4. **Save as prefab** in your Prefabs folder

### Step 2: Set Up Boss Drops

1. **Select your RobotBoss** in the scene
2. **Add BossDropSystem component**
3. **Configure drops:**

   **Guaranteed Drops:**
   - Size: 1
   - Element 0:
     - Item Prefab: [Your Tome Prefab]
     - Drop Chance: 1.0 (100%)
     - Min Quantity: 1
     - Max Quantity: 1
     - Item Name: "Blue Tome"

   **Random Drops** (optional):
   - Add more items with different drop chances
   - Example: Health potions with 0.7 (70%) chance

   **Drop Physics:**
   - Drop Radius: 2 (how spread out items land)
   - Drop Height: 2 (how high above boss to start)
   - Ground Layer: [Set to your ground layer]
   
   **Drop Timing:**
   - Drop Delay: 0.5 (delay after boss death)
   - Drop Interval: 0.1 (time between items)

### Step 3: Test the System

1. **Play the game**
2. **Defeat the boss**
3. **Watch items drop in arcs and spin**
4. **Collect them to see the effects**

## Visual Effects

### Basic Glow Effect
The items automatically glow and pulse. You can customize:
- `glowColor` - Color of the glow
- `glowIntensityMin/Max` - How bright the pulse gets
- `floatSpeed` - How fast the bobbing/pulsing

### Particle Effects (Optional)
To add magic particles to your tome:
1. **Add a Particle System** as child of tome prefab
2. **Assign it to `magicParticles`** in TomePickup
3. **Configure particle settings** for magical effect

## Advanced Customization

### Custom Item Types
Create new item scripts that extend `ItemDrop`:

```csharp
public class WeaponPickup : ItemDrop
{
    public WeaponData weaponData;
    
    protected override void OnItemCollected(GameObject player)
    {
        // Custom weapon pickup logic
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddWeapon(weaponData);
        }
    }
}
```

### Multiple Drop Types
You can add different items to the boss drops:
- Health potions (using existing PotionPickup system)
- Gold coins
- Equipment pieces
- Ability scrolls

### Drop Chances
Configure different rarities:
- Common items: 0.8-1.0 (80-100%)
- Uncommon items: 0.3-0.5 (30-50%)
- Rare items: 0.05-0.1 (5-10%)
- Legendary items: 0.01 (1%)

## Integration with Game Systems

### Experience System
```csharp
// In TomePickup.GrantExperience()
PlayerProgression progression = player.GetComponent<PlayerProgression>();
if (progression != null)
{
    progression.GainExperience(experienceValue);
}
```

### Currency System
```csharp
// In TomePickup.GrantGold()
GameManager.Instance.AddGold(goldValue);
```

### UI Messages
```csharp
// In TomePickup.ShowCollectionMessage()
UIManager.Instance.ShowFloatingText($"+{experienceValue} XP", transform.position);
```

## Troubleshooting

**Items not dropping?**
- Check if BossDropSystem is added to boss
- Verify itemPrefab is assigned in drops list
- Check ground layer settings

**Items falling through ground?**
- Ensure ground has proper colliders
- Check groundLayer mask in BossDropSystem
- Verify ground layer assignment

**No pickup effects?**
- Check if pickupEffect prefab is assigned
- Verify AudioManager sound indices are correct
- Ensure player has proper tag ("Player")

**Items not spinning/glowing?**
- Verify SpriteRenderer is attached to prefab
- Check if TomePickup component has correct settings
- Ensure enableGlow is checked

## Performance Notes

- Items automatically clean up after collection
- Coroutines stop when items are destroyed
- Particle effects have lifetime limits
- Consider object pooling for many drops

## Future Enhancements

- Add rarity-based glow colors
- Implement magnetic collection (items fly to player)
- Add sound variation based on item type
- Create collection UI with item icons
- Add inventory system integration 