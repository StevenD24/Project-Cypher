using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth,
        maxHealth,
        damageAmount;
    public HealthBar healthBar;
    public float immortalTime = 2f;
    private float immortalCounter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(currentHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (immortalCounter > 0)
        {
            immortalCounter -= Time.deltaTime;
        }
    }

    public void DealDamage()
    {
        if (immortalCounter <= 0)
        {
            currentHealth -= damageAmount;
            healthBar.SetHealth(currentHealth);
            if (currentHealth <= 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                immortalCounter = immortalTime;
            }
        }
    }
}
