using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth,
        maxHealth,
        damageAmount;
    public HealthBar healthBar;
    public float immortalTime = 1f;
    private float immortalCounter;
    public GameObject immortalEffect;
    public int healthPotionIncrement = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(currentHealth);
        immortalEffect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (immortalCounter > 0)
        {
            immortalCounter -= Time.deltaTime;

            if (immortalCounter <= 0)
            {
                immortalEffect.SetActive(false);
            }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Bonus Bottle")
        {
            immortalCounter = immortalTime + 2;
            Destroy(collision.gameObject);
            immortalEffect.SetActive(true);
        }
        
        if (collision.gameObject.tag == "Health Potion")
        {
            if (currentHealth < maxHealth) {
                currentHealth += healthPotionIncrement;
                healthBar.SetHealth(currentHealth);
            }
            Destroy(collision.gameObject);
        }
    }
}
