using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth, maxHealth, damageAmount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DealDamage() {
        currentHealth -= damageAmount;
        if (currentHealth <= 0) {
            gameObject.SetActive(false);
        }
    }
}
