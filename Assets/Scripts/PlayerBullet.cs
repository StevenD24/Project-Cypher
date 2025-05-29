using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float bulletSpeed;
    private Rigidbody2D rb;
    private Vector2 bulletDirection; // Store the direction once

    private Player playerController;
    private GameObject playerObject;

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
}
