using UnityEngine;

public class EnemyBulletSimple : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;

    private void Start()
    {
        // Destroy bullet after 5 seconds if it doesn't hit anything
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        // Move the bullet forward
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // Damage the player
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.DealDamage();
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Wall")
        {
            // Destroy bullet when hitting walls/ground
            Destroy(gameObject);
        }
    }
}
