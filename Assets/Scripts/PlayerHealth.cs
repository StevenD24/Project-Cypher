using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth, maxHealth, damageAmount;
    public HealthBar healthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(currentHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DealDamage() {
        currentHealth -= damageAmount;
        healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0) {
            gameObject.SetActive(false);
        }
    }
}
