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
    private bool wasGrounded; // Track previous grounded state

    [Header("Double Jump")]
    public int maxAirJumps = 1; // Allow only 1 air jump
    private int airJumpsRemaining;

    private Animator animator;
    private bool isFacingRight = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        airJumpsRemaining = maxAirJumps; // Initialize with max air jumps
    }

    // Update is called once per frame
    void Update()
    {
        wasGrounded = isGrounded; // Store previous grounded state
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Reset jumps only when landing (transitioning from air to ground)
        if (isGrounded && !wasGrounded)
        {
            airJumpsRemaining = maxAirJumps;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Allow jumping if grounded OR if air jumps remaining
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                // Can always jump from ground
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                AudioManager.instance.PlaySFX(4);
            }
            else if (airJumpsRemaining > 0)
            {
                // Can only air jump if air jumps remaining
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                airJumpsRemaining--; // Use up one air jump
                AudioManager.instance.PlaySFX(4);
            }
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
