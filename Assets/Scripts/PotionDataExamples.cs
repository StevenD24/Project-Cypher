using UnityEngine;

// This is an example script showing how to create different potion types
// You can use this as reference when creating PotionData assets in the editor
public class PotionDataExamples : MonoBehaviour
{
    [Header("Example Potion Configurations")]
    [TextArea(3, 10)]
    public string instructions =
        @"
To create potion data assets:
1. Right-click in Project window
2. Go to Create > Potions > Potion Data
3. Configure the settings based on examples below:

HEALTH POTION:
- Potion Type: Health
- Effect Value: 25 (health points to restore)
- Effect Duration: 0 (instant effect)
- Glow Tint: Red (1, 0.7, 0.7, 1)
- Audio Clip Index: 1

MANA POTION:
- Potion Type: Mana
- Effect Value: 30 (mana points to restore)
- Effect Duration: 0 (instant effect)
- Glow Tint: Blue (0.7, 0.7, 1, 1)
- Audio Clip Index: 1

IMMUNITY POTION:
- Potion Type: Immunity
- Effect Value: 0 (not used for immunity)
- Effect Duration: 5.0 (seconds of immunity)
- Glow Tint: Yellow (1, 1, 0.7, 1)
- Audio Clip Index: 2

SPEED POTION:
- Potion Type: Speed
- Effect Value: 50 (50% speed increase)
- Effect Duration: 10.0 (seconds)
- Glow Tint: Cyan (0.7, 1, 1, 1)
- Audio Clip Index: 1

STRENGTH POTION:
- Potion Type: Strength
- Effect Value: 2 (additional strength points)
- Effect Duration: 15.0 (seconds)
- Glow Tint: Magenta (1, 0.7, 1, 1)
- Audio Clip Index: 1
";

    // Example method showing how to create potion data at runtime (for reference only)
    void CreateExampleHealthPotion()
    {
        // This is just for reference - normally you'd create these as assets in the editor
        PotionData healthPotion = ScriptableObject.CreateInstance<PotionData>();
        healthPotion.potionName = "Health Potion";
        healthPotion.potionType = PotionType.Health;
        healthPotion.effectValue = 25;
        healthPotion.effectDuration = 0f; // Instant effect
        healthPotion.glowTint = new Color(1f, 0.7f, 0.7f, 1f);
        healthPotion.audioClipIndex = 1;
        healthPotion.effectDuration_Visual = 2f;
    }
}
