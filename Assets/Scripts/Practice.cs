using UnityEngine;

public class Practice : MonoBehaviour
{
    // Variables
    // Types of variables: int, float, string, booleans, GameObjects
    // public/private type name val;
    // public int moveSpeed = 0;
    // public int A = 5;
    // public int B = 3;
    // private int C;

    // // bread, soda, cost
    // public float bread = 5.99F;
    // public float soda = 1.49F;
    // public float cost;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    public float moveSpeed = 5f;
    private float horizontalInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Script has started!");
        // C = A + B;
        // Debug.Log("Number is: " + C);

        // AddTwoNumbers(bread, soda);

        // destroy after 3 seconds
        // Destroy(gameObject, 3f);
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    // public void AddTwoNumbers(float a, float b)
    // {
    //     cost = a + b;
    //     Debug.Log("The total cost is: " + cost);
    // }

    private void OnMouseDown()
    {
        Debug.Log("Mouse clicked on object!");
        rb.bodyType = RigidbodyType2D.Dynamic;
        sr.color = Color.green;
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        Debug.Log("We have collided with the ground");
        Destroy(collision.gameObject, 2f);
    }

    private void OnTriggerEnter2D(Collider collision)
    {   
        Debug.Log("We collided with the circle");
    }
}
