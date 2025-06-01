using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    public float moveSpeed = 5f;
    private float horizontalInput;
    public float jumpForce;

    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    private bool isGrounded;

    private Animator animator;
    private bool isFacingRight = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        horizontalInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            AudioManager.instance.PlaySFX(4);
        }

        if (rb.linearVelocity.x > 0)
        {
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
            isFacingRight = true;
        }
        else if (rb.linearVelocity.x < 0)
        {
            transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
            isFacingRight = false;
        }

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    public bool IsFacingRight()
    {
        return isFacingRight;
    }
}
