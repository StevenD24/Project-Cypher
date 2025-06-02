using System.Collections;
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

    [Header("Dashing")]
    public float dashSpeed = 15f; // Speed of the dash
    public float dashDuration = 0.15f; // How long the dash lasts (made shorter)
    public float dashCooldown = 1f; // Cooldown between dashes
    private bool isDashing = false;
    private bool canDash = true;
    private float dashTimeRemaining = 0f;

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

        // Handle dashing input
        HandleDashInput();

        // Handle dash mechanics
        HandleDash();

        // Only allow normal movement if not dashing
        if (!isDashing)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        // Allow jumping if grounded OR if air jumps remaining (but not while dashing)
        if (Input.GetButtonDown("Jump") && !isDashing)
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

        // Handle facing direction (only if not dashing to prevent flip during dash)
        if (!isDashing)
        {
            if (rb.linearVelocity.x > 0)
            {
                transform.localScale = new Vector3(
                    1f,
                    transform.localScale.y,
                    transform.localScale.z
                );
                isFacingRight = true;
            }
            else if (rb.linearVelocity.x < 0)
            {
                transform.localScale = new Vector3(
                    -1f,
                    transform.localScale.y,
                    transform.localScale.z
                );
                isFacingRight = false;
            }
        }

        // Update animator parameters - dashing takes absolute precedence
        if (isDashing)
        {
            // During dash, force all parameters to ensure dash animation plays
            animator.SetBool("isDashing", true);
            animator.SetBool("isGrounded", false); // Prevent ground-based animations
            animator.SetFloat("Speed", 0f); // Prevent movement-based animations

            // Debug to see if animation is being set
            Debug.Log(
                "DASH: Setting isDashing to true, current animation: "
                    + animator.GetCurrentAnimatorStateInfo(0).IsName("Dash")
            );
        }
        else
        {
            // Only update other animation parameters when not dashing
            animator.SetBool("isDashing", false);
            animator.SetBool("isGrounded", isGrounded);
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }
    }

    private void HandleDashInput()
    {
        // Check for dash input (E key only) - dash in player's facing direction, only when not grounded (in air)
        if (canDash && !isDashing && !isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Dash in the direction the player is facing
                int dashDirection = isFacingRight ? 1 : -1;
                StartDash(dashDirection);
            }
        }
    }

    private void StartDash(int direction)
    {
        isDashing = true;
        canDash = false;
        dashTimeRemaining = dashDuration;

        // Play dash sound effect
        AudioManager.instance.PlaySFX(5); // Adjust the index based on your sound effects

        // Set dash velocity (preserve Y velocity to allow air dashing)
        rb.linearVelocity = new Vector2(direction * dashSpeed, rb.linearVelocity.y);

        // Face the dash direction
        if (direction > 0)
        {
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
            isFacingRight = true;
        }
        else
        {
            transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
            isFacingRight = false;
        }

        // Start cooldown coroutine
        StartCoroutine(DashCooldown());
    }

    private void HandleDash()
    {
        if (isDashing)
        {
            dashTimeRemaining -= Time.deltaTime;

            if (dashTimeRemaining <= 0f)
            {
                EndDash();
            }
        }
    }

    private void EndDash()
    {
        isDashing = false;
        // Reduce horizontal velocity after dash ends (optional - for smoother transition)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);

        // Debug to help with animation troubleshooting
        Debug.Log("DASH ENDED: Setting isDashing to false");
    }

    private System.Collections.IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public bool IsFacingRight()
    {
        return isFacingRight;
    }

    public bool GetIsDashing()
    {
        return isDashing;
    }
}
