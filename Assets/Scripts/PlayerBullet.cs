using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float bulletSpeed;
    private Rigidbody2D rb;
    private Vector2 bulletDirection; // Store the direction once

    private Player playerController;
    private GameObject playerObject;

    public GameObject hitEffect;

    private void Awake()
    {
        Destroy(gameObject, 2f);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObject.GetComponent<Player>();

        rb = GetComponent<Rigidbody2D>();

        // Set the bullet direction ONCE when bullet is created
        if (playerController.IsFacingRight())
        {
            bulletDirection = transform.right; // Moving right
        }
        else
        {
            bulletDirection = -transform.right; // Moving left
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Use the stored direction, don't check player direction again
        rb.linearVelocity = bulletDirection * bulletSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Instantiate(hitEffect, collision.transform.position, collision.transform.rotation);
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.takeDamage();
            }

            Destroy(gameObject);
        }
        else if (
            collision.gameObject.tag == "RobotBoss"
            || collision.gameObject.GetComponent<RobotBoss>() != null
        )
        {
            Instantiate(hitEffect, collision.transform.position, collision.transform.rotation);
            RobotBoss robotBoss = collision.GetComponent<RobotBoss>();
            if (robotBoss != null)
            {
                robotBoss.TakeDamage(10); // Adjust damage amount as needed
            }

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only destroy if hitting ground, walls, or other solid objects
        // Don't destroy if hitting the player
        if (collision.gameObject.tag != "Player")
        {
            Destroy(gameObject);
        }
    }
}
