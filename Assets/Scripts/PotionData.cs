using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "Potions/Potion Data")]
public class PotionData : ScriptableObject
{
    [Header("Basic Info")]
    public string potionName;
    public PotionType potionType;
    public Sprite potionSprite;

    [Header("Effect Values")]
    public int effectValue; // Health amount, mana amount, etc.
    public float effectDuration; // For temporary effects like immunity

    [Header("Visual Effects")]
    public Color glowTint = Color.white;
    public GameObject pickupEffect; // Particle effect when picked up
    public float effectDuration_Visual = 2f; // How long pickup effect lasts

    [Header("Audio")]
    public int audioClipIndex = 1; // Index for AudioManager

    [Header("Animation Settings")]
    public bool enableCustomBobbing = false;
    public float bobbingHeight = 0.05f;
    public float bobbingPeriod = 2.5f;
}

public enum PotionType
{
    Health,
    Mana,
    Immunity,
    Speed,
    Strength,
    Poison, // For enemies or negative effects
    Magic,
    Experience,
    Shield,
}
