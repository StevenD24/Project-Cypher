using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform pointA,
        pointB;
    public int Speed;
    private Vector3 currentTarget;
    private SpriteRenderer sr;
    public int currentHealth, maxHealth, damageAmount;
    private Animator animator;
    public EnemyHealthBar enemyHealthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        enemyHealthBar.SetMaxHealth(currentHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position == pointA.position)
        {
            currentTarget = pointB.position;
            sr.flipX = false;
        }
        else if (transform.position == pointB.position)
        {   
            sr.flipX = true;
            currentTarget = pointA.position;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            Speed * Time.deltaTime
        );
    }

    public void takeDamage()
    {
        currentHealth -= damageAmount;
        enemyHealthBar.SetHealth(currentHealth);

        if (currentHealth <= 0) {
            Death();
        }
    }

    public void Death()
    {
        Debug.Log("Enemy is dead");
        animator.SetBool("isDead", true);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Destroy(gameObject, 0.15f);
    }
}
