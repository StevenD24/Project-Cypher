using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    public void SetHealth(int health)
    {
        slider.value = health;

        // Hide the green fill when health is 0
        if (health <= 0 && slider.fillRect != null)
        {
            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                Color fillColor = fillImage.color;
                fillColor.a = 0f; // Set alpha to 0 to make it transparent
                fillImage.color = fillColor;
            }
        }
    }
}
