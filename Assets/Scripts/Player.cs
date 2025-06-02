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

    [Header("One-Way Platforms")]
    public float platformDetectionDistance = 2f; // How far to check for platforms above
    public string platformTag = "OneWayPlatform"; // Tag for one-way platforms
    private Collider2D playerCollider;

    private Animator animator;
    private bool isFacingRight = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        airJumpsRemaining = maxAirJumps; // Initialize with max air jumps
    }

    // Update is called once per frame
    void Update()
    {
        wasGrounded = isGrounded; // Store previous grounded state

        // Check for regular ground
        bool groundedOnRegularGround = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Check for one-way platforms
        bool groundedOnOneWayPlatform = IsGroundedOnOneWayPlatform();

        // Player is grounded if on either regular ground or one-way platform
        isGrounded = groundedOnRegularGround || groundedOnOneWayPlatform;

        // Reset jumps only when landing (transitioning from air to ground)
        if (isGrounded && !wasGrounded)
        {
            airJumpsRemaining = maxAirJumps;
        }

        // Handle one-way platforms
        HandleOneWayPlatforms();

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

    private void HandleOneWayPlatforms()
    {
        // Find all GameObjects with the OneWayPlatform tag
        GameObject[] platforms = GameObject.FindGameObjectsWithTag(platformTag);

        foreach (GameObject platformObj in platforms)
        {
            Collider2D platformCollider = platformObj.GetComponent<Collider2D>();
            if (platformCollider == null)
                continue;

            // Check player position relative to platform
            bool playerBelowPlatform = transform.position.y < platformCollider.bounds.center.y;
            bool playerAbovePlatform = transform.position.y > platformCollider.bounds.max.y + 0.1f;
            bool playerMovingUp = rb.linearVelocity.y > 0.1f; // Small threshold to avoid jittering
            bool playerMovingDown = rb.linearVelocity.y < -0.1f;

            // Check if player is pressing down (for drop-through functionality)
            bool playerPressingDown = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

            if (playerBelowPlatform && playerMovingUp)
            {
                // Player is below and jumping up - disable collision temporarily
                Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
                // Re-enable collision after a short delay when player stops moving up
                StartCoroutine(ReEnableCollisionWhenNotMovingUp(platformCollider));
            }
            else if (!playerBelowPlatform && playerPressingDown)
            {
                // Player is on platform and wants to drop through
                Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
                // Start a coroutine to re-enable collision after player falls below platform
                StartCoroutine(ReEnableCollisionWhenBelow(platformCollider));
            }
            else if (playerAbovePlatform && (playerMovingDown || rb.linearVelocity.y <= 0))
            {
                // Player is clearly above platform and falling/stationary - ensure collision is enabled
                Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
            }
        }
    }

    private System.Collections.IEnumerator ReEnableCollisionWhenNotMovingUp(
        Collider2D platformCollider
    )
    {
        // Wait until player stops moving up or is clearly above the platform
        while (
            rb.linearVelocity.y > 0 || transform.position.y < platformCollider.bounds.max.y + 0.2f
        )
        {
            yield return new WaitForFixedUpdate();
        }

        // Re-enable collision
        if (platformCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }

    private System.Collections.IEnumerator ReEnableCollisionWhenBelow(Collider2D platformCollider)
    {
        // Wait a small amount of time to ensure player starts falling
        yield return new WaitForSeconds(0.1f);

        // Wait until player is clearly below the platform
        while (transform.position.y > platformCollider.bounds.min.y - 0.5f)
        {
            yield return new WaitForFixedUpdate();
        }

        // Re-enable collision
        if (platformCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
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

    private bool IsGroundedOnOneWayPlatform()
    {
        // Check for one-way platforms at the ground check position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            groundCheck.position,
            groundCheckRadius
        );

        foreach (Collider2D collider in colliders)
        {
            // Check if this collider belongs to a one-way platform
            if (collider.gameObject.CompareTag(platformTag))
            {
                // Make sure the player is on top of the platform (not passing through)
                if (transform.position.y > collider.bounds.max.y - 0.1f)
                {
                    // Also make sure collision is enabled between player and platform
                    if (!Physics2D.GetIgnoreCollision(playerCollider, collider))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
