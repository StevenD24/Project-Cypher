using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class RobotBoss : MonoBehaviour
{
    [Header("Movement")]
    public Transform pointA;
    public Transform pointB;
    public int speed;
    private Vector3 currentTarget;

    [Header("Health & Combat")]
    public int currentHealth;
    public int maxHealth;
    public int damageAmount;
    public EnemyHealthBar enemyHealthBar;

    [Header("Detection, Chase & Attack Settings")]
    public float detectingRange = 8f;
    public float timeBetweenAttacks = 2f;
    public float attackRange = 4f;
    public float aggroTime = 3f; // How long to stay aggro after taking damage

    [Header("Jump Attack Settings")]
    public GameObject jumpEffectPrefab; // Drag your jump effect prefab here
    public float jumpSpeed = 10f;
    public float jumpDamage = 30f;
    public float jumpRange = 15f; // Maximum range for jump attack
    public float jumpCooldown = 5f; // Cooldown between jump attacks
    public float jumpAOERadius = 4.2f; // Inner AOE radius for full jump damage (reduced from 5f)
    public float jumpOuterAOERadius = 6f; // Outer AOE radius for reduced damage (reduced from 7f)
    public float jumpOuterDamage = 15f; // Reduced damage for outer AOE
    public LayerMask playerLayerMask = 1; // Layer mask for player detection

    [Header("Health Thresholds")]
    public float tiredHealthThreshold = 0.2f; // Health percentage when robot becomes tired (20%)

    [Header("Healing Mechanic")]
    public GameObject healingEffectPrefab; // Drag your healing effect prefab here
    public float healingDuration = 3f; // How long the healing state lasts
    public float healingCooldown = 30f; // Cooldown between healing attempts
    public float healingDelay = 0.5f; // Delay before entering healing state
    public int healingAmountPerTick = 10; // Health restored per healing tick

    [Header("Spine2D Animations")]
    [SpineAnimation]
    public string idle_1 = "idle";

    [SpineAnimation]
    public string idle_2 = "idle_2";

    [SpineAnimation]
    public string walk_1 = "walk";

    [SpineAnimation]
    public string walk_2 = "walk_tired";

    [SpineAnimation]
    public string flag_meeting = "flag_meeting";

    [SpineAnimation]
    public string jump = "jump";

    [SpineAnimation]
    public string run_1 = "run";

    [SpineAnimation]
    public string run_2 = "skip";

    [SpineAnimation]
    public string attack = "shot_sttack";

    [SpineAnimation]
    public string getHit = "hit";

    [SpineAnimation]
    public string death = "dead";

    [SpineAnimation]
    public string skill_1 = "shot_skill_heavy";

    [SpineAnimation]
    public string skill_2 = "shot_skill_continues";

    // Spine components
    private SkeletonAnimation skeletonAnimation;
    private Spine.AnimationState spineAnimationState;
    private Spine.Skeleton skeleton;

    // Color management for flashing effect (Spine2D)
    private Color originalSkeletonColor = Color.white;
    private bool isFlashing = false;

    // State tracking
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isAggroed = false; // Robot is aggroed and will chase regardless of range
    private float aggroEndTime = 0f; // When aggro state should end
    private GameObject player;
    private string currentAnimation = "";
    private float lastAttackTime = -999f; // Track when last attack happened
    private float lastJumpAttackTime = -999f; // Track when last jump attack happened
    private bool isJumping = false; // Track if currently performing jump attack
    private bool isHealing = false; // Track if currently in healing state
    private bool isInvincible = false; // Track if robot is invincible
    private float lastHealingTime = -999f; // Track when last healing happened
    private float healingTriggerTime = 0f; // When healing should trigger
    private bool bossEncounterStarted = false; // Track if boss encounter has begun

    [Header("Ground Detection")]
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayerMask = 1; // Set this to your ground layer
    private bool isGrounded = false;
    private Transform groundCheck;

    void Start()
    {
        // Initialize Spine2D components
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation != null)
        {
            spineAnimationState = skeletonAnimation.AnimationState;
            skeleton = skeletonAnimation.Skeleton;
        }

        // Find GroundCheck child object (just like player)
        groundCheck = transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            Debug.LogWarning(
                "GroundCheck child object not found! Please create a GroundCheck child object positioned at the robot's feet."
            );
        }

        // Initialize health
        currentHealth = maxHealth;
        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetMaxHealth(currentHealth);
        }

        player = GameObject.FindGameObjectWithTag("Player");

        // Play initial idle animation
        PlayAnimation(idle_1);
    }

    void Update()
    {
        if (isDead || player == null)
            return;

        CheckGrounded();
        CalculateDistance();
    }

    private void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayerMask
            );
        }
        else
        {
            // Fallback: use robot's position with slight offset downward
            Vector2 checkPosition = new Vector2(transform.position.x, transform.position.y - 0.5f);
            isGrounded = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayerMask);
        }
    }

    private bool IsPlayerAlive()
    {
        return player != null && player.activeInHierarchy;
    }

    private string GetRunAnimation()
    {
        // Use tired walk animation when health is below threshold
        float healthPercentage = (float)currentHealth / maxHealth;
        return healthPercentage < tiredHealthThreshold ? walk_2 : run_1;
    }

    private int GetCurrentSpeed()
    {
        // Use speed of 1 when health is below threshold
        float healthPercentage = (float)currentHealth / maxHealth;
        return healthPercentage < tiredHealthThreshold ? 1 : speed;
    }

    private void CalculateDistance()
    {
        // If currently attacking, jumping, or healing, let the action finish even if player dies
        if (isAttacking || isJumping || isHealing)
        {
            Debug.Log(
                "Currently attacking, jumping, or healing - letting action finish regardless of player state"
            );
            return;
        }

        // Check if player exists and is alive before doing anything
        if (!IsPlayerAlive())
        {
            Debug.Log("Player is null or dead - stopping all actions");
            PlayAnimation(idle_1);
            // Return to normal music if boss was previously aggro
            if (isAggroed && AudioManager.instance != null)
            {
                AudioManager.instance.PlayNormalBGM();
                isAggroed = false;
            }
            return;
        }

        // HIGHEST PRIORITY: Check for healing condition - can interrupt other actions
        float healthPercentage = (float)currentHealth / maxHealth;
        float timeSinceLastHealing = Time.time - lastHealingTime;

        if (
            healthPercentage < tiredHealthThreshold
            && timeSinceLastHealing >= healingCooldown
            && !isHealing
        )
        {
            // Set trigger time for delayed healing
            if (healingTriggerTime == 0f)
            {
                healingTriggerTime = Time.time + healingDelay;
                Debug.Log(
                    $"Healing will trigger in {healingDelay} seconds - prioritizing over combat!"
                );
                // Start preparing animation for smoother transition
                PlayAnimation(idle_2);
            }
            // Check if delay has passed
            else if (Time.time >= healingTriggerTime)
            {
                Debug.Log(
                    $"HEALING PRIORITY ACTIVATED! Health: {healthPercentage:P}, Cooldown: {timeSinceLastHealing:F1}s"
                );
                StartCoroutine(PerformHealing());
                healingTriggerTime = 0f; // Reset trigger time
                return;
            }
            // During healing delay, show preparing animation
            else
            {
                Debug.Log(
                    $"Preparing to heal in {healingTriggerTime - Time.time:F1}s - showing preparation"
                );
                // Keep playing preparation animation
                PlayAnimation(idle_2);
                return;
            }
        }
        else if (healthPercentage >= tiredHealthThreshold)
        {
            // Reset healing trigger if health goes above threshold
            healingTriggerTime = 0f;
        }

        // Check for jump attack condition (health below 50% and cooldown has passed)
        float distance = Vector2.Distance(transform.position, player.transform.position);
        float timeSinceLastJump = Time.time - lastJumpAttackTime;

        if (
            healthPercentage < 0.7f
            && timeSinceLastJump >= jumpCooldown
            && distance <= jumpRange
            && !isJumping
            && !isAttacking
        )
        {
            Debug.Log(
                $"Jump attack triggered! Health: {healthPercentage:P}, Distance: {distance:F1}, Cooldown: {timeSinceLastJump:F1}s, isJumping: {isJumping}"
            );
            StartCoroutine(PerformJumpAttack());
            return;
        }
        else if (healthPercentage < 0.5f && timeSinceLastJump < jumpCooldown)
        {
            Debug.Log(
                $"Jump attack on cooldown - Health: {healthPercentage:P}, Need {jumpCooldown - timeSinceLastJump:F1}s more"
            );
        }

        // Check if aggro should expire
        if (isAggroed && Time.time > aggroEndTime)
        {
            isAggroed = false;
            Debug.Log("Aggro expired - returning to normal behavior");
            // Note: Don't change music here - boss encounter continues until death
        }

        Debug.Log(
            $"Distance: {distance:F1} | DetectRange: {detectingRange} | AttackRange: {attackRange} | isAttacking: {isAttacking} | isAggroed: {isAggroed}"
        );

        // Don't move while attacking
        if (isAttacking)
        {
            Debug.Log("Currently attacking - not moving");
            return;
        }

        // Robot should chase if player is in range OR if robot is aggroed
        if (distance < detectingRange || isAggroed)
        {
            Debug.Log(
                isAggroed
                    ? "Robot is aggroed - chasing regardless of range!"
                    : "Player in detect range - should chase!"
            );

            // Trigger boss music when encounter starts (only once)
            if (!bossEncounterStarted && AudioManager.instance != null)
            {
                AudioManager.instance.PlayBossBGM();
                bossEncounterStarted = true;
                Debug.Log("Boss encounter started - playing boss music!");
            }

            // Check if enough time has passed since last attack
            float timeSinceLastAttack = Time.time - lastAttackTime;

            if (
                distance <= attackRange
                && !isAttacking
                && timeSinceLastAttack >= timeBetweenAttacks
            )
            {
                Debug.Log(
                    $"Player in attack range - attacking! (Time since last attack: {timeSinceLastAttack:F1}s)"
                );
                // Don't record attack time here - do it at the end of the attack
                // Stop running and start attack
                FacePlayer();
                PlayAnimation(skill_1);
                StartCoroutine(AttackAfterDelay());
            }
            else if (!isAttacking)
            {
                if (distance <= attackRange && timeSinceLastAttack < timeBetweenAttacks)
                {
                    Debug.Log(
                        $"Player in attack range but still cooling down (Need {timeBetweenAttacks - timeSinceLastAttack:F1}s more)"
                    );
                    // Face player and wait - don't move
                    FacePlayer();
                    PlayAnimation(idle_2);
                }
                else if (distance > attackRange)
                {
                    Debug.Log("Chasing player!");
                    // Only chase if grounded and player is outside attack range
                    if (isGrounded)
                    {
                        // Play run animation and move towards player
                        PlayAnimation(GetRunAnimation());
                        ChasePlayer();
                    }
                    else
                    {
                        Debug.Log("Robot is in air - cannot chase (playing idle instead)");
                        PlayAnimation(idle_1);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Player too far - idling");
            PlayAnimation(idle_1);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (spineAnimationState != null && !string.IsNullOrEmpty(animationName))
        {
            // Don't allow any animation to override the death animation
            if (isDead && currentAnimation == death && animationName != death)
            {
                Debug.Log(
                    $"Robot is dead - ignoring animation change from '{death}' to '{animationName}'"
                );
                return;
            }

            if (currentAnimation != animationName)
            {
                try
                {
                    var animation = spineAnimationState.Data.SkeletonData.FindAnimation(
                        animationName
                    );
                    if (animation != null)
                    {
                        // Death animation should not loop, all others should loop
                        bool shouldLoop = animationName != death;
                        spineAnimationState.SetAnimation(0, animationName, shouldLoop);
                        currentAnimation = animationName;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error playing animation '{animationName}': {e.Message}");
                }
            }
        }
    }

    private void FacePlayer()
    {
        // Check if player exists and is alive before trying to face them
        if (!IsPlayerAlive())
        {
            Debug.Log("Player is null or dead - cannot face player");
            return;
        }

        Vector2 playerPosition = new Vector2(player.transform.position.x, transform.position.y);
        Vector2 direction = (playerPosition - (Vector2)transform.position).normalized;

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(0.65f, 0.65f, 1);
            Debug.Log("Facing right towards player");
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-0.65f, 0.65f, 1);
            Debug.Log("Facing left towards player");
        }
    }

    private void ChasePlayer()
    {
        // Check if player exists and is alive before chasing
        if (!IsPlayerAlive())
        {
            Debug.Log("Player is null or dead - cannot chase");
            return;
        }

        // Don't move if not grounded
        if (!isGrounded)
        {
            Debug.Log("Robot is not grounded - cannot chase (preventing air running)");
            // Play idle animation when in air
            PlayAnimation(idle_1);
            return;
        }

        Vector2 playerPosition = new Vector2(player.transform.position.x, transform.position.y);
        Vector2 direction = (playerPosition - (Vector2)transform.position).normalized;

        Debug.Log(
            $"Chasing: Robot at {transform.position.x:F1}, Player at {player.transform.position.x:F1}, Direction: {direction.x:F2}, Grounded: {isGrounded}"
        );

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(0.65f, 0.65f, 1);
            Debug.Log("Facing right");
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-0.65f, 0.65f, 1);
            Debug.Log("Facing left");
        }

        Vector2 movement = direction * GetCurrentSpeed() * Time.deltaTime;
        transform.Translate(movement);
        Debug.Log($"Moving by: {movement}, New position: {transform.position.x:F1}");
    }

    IEnumerator AttackAfterDelay()
    {
        isAttacking = true;
        Debug.Log("Attack started - robot should stop moving");

        // Play attack animation
        PlayAnimation(skill_1);

        // Deal damage to player if still in range and player exists
        if (IsPlayerAlive())
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= attackRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.DealDamage(damageAmount);
                    Debug.Log($"Robot dealt {damageAmount} damage to player!");
                }
                else
                {
                    Debug.LogWarning("Player has no PlayerHealth component!");
                }
            }
            else
            {
                Debug.Log("Player moved out of attack range - no damage dealt");
            }
        }
        else
        {
            Debug.Log("Player is null or dead - cannot deal damage");
        }

        // Wait for attack animation to finish (short duration)
        float attackAnimationTime = 0.8f; // Attack animation duration
        yield return new WaitForSeconds(attackAnimationTime);

        // Check if robot died during attack - if so, stop the coroutine
        if (isDead)
        {
            Debug.Log("Robot died during attack - stopping attack coroutine");
            yield break;
        }

        // Switch to idle during cooldown period
        Debug.Log("Attack animation finished - switching to idle for cooldown");
        PlayAnimation(idle_2);

        // Wait for remaining cooldown time
        float cooldownTime = timeBetweenAttacks - attackAnimationTime;
        if (cooldownTime > 0)
        {
            Debug.Log($"Cooling down for {cooldownTime:F1} more seconds");
            yield return new WaitForSeconds(cooldownTime);
        }

        // Check again if robot died during cooldown
        if (isDead)
        {
            Debug.Log("Robot died during cooldown - stopping attack coroutine");
            yield break;
        }

        isAttacking = false;
        Debug.Log("Attack cooldown finished - ready for next attack");

        // Record attack time at the end so cooldown starts from completion
        lastAttackTime = Time.time;
    }

    IEnumerator PerformJumpAttack()
    {
        if (!IsPlayerAlive() || isDead)
            yield break;

        isJumping = true;
        Debug.Log("Starting jump attack!");

        // Store original position and target position (keep same Y level)
        Vector3 startPosition = transform.position;
        float originalY = startPosition.y; // Store the original Y position

        // FIX: Commit to the player's position when jump starts - don't update during jump
        // ADVANCED PREDICTION: Account for dash mechanics and player behavior patterns
        Vector3 playerVelocity = Vector3.zero;
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        Player playerScript = player.GetComponent<Player>();
        bool playerIsGrounded = true; // Default assumption
        bool playerCanDash = true; // Default assumption
        bool playerIsDashing = false;

        if (playerRb != null)
        {
            playerVelocity = playerRb.linearVelocity;
        }

        if (playerScript != null)
        {
            playerIsDashing = playerScript.GetIsDashing();
            // If we had access to player's grounded state and dash cooldown, we'd use them here
            // For now, assume player can dash if not currently dashing and has reasonable velocity
        }

        // ADVANCED PREDICTION LOGIC
        Vector3 basePlayerPosition = player.transform.position;
        float jumpDuration = 0.7f; // Match the actual jump duration

        // Calculate multiple potential target positions
        Vector3 primaryTarget;

        if (playerIsDashing)
        {
            // If player is currently dashing, predict where dash will end
            float remainingDashTime = 0.15f; // Assume worst case - full dash duration remaining
            Vector3 dashEndPosition =
                basePlayerPosition + (playerVelocity.normalized * 15f * remainingDashTime);
            primaryTarget = new Vector3(dashEndPosition.x, originalY, transform.position.z);
            Debug.Log($"Player is dashing - targeting dash end position: {primaryTarget}");
        }
        else
        {
            // Advanced prediction for non-dashing player
            float basePredictionTime = jumpDuration * 0.8f; // Predict for most of jump duration

            // Consider if player might dash during our jump
            bool playerLikelyToDash = !playerIsGrounded && Mathf.Abs(playerVelocity.x) > 2f; // Moving fast in air

            if (playerLikelyToDash)
            {
                // Create a spread of potential positions accounting for dash
                Vector3 currentTrajectory =
                    basePlayerPosition + (playerVelocity * basePredictionTime);

                // Calculate potential dash positions in both directions
                float dashDistance = 15f * 0.15f; // Max dash distance
                Vector3 leftDashTarget = currentTrajectory + Vector3.left * dashDistance;
                Vector3 rightDashTarget = currentTrajectory + Vector3.right * dashDistance;

                // Choose target based on player's current movement and position relative to robot
                float distanceToRobot = Vector2.Distance(transform.position, basePlayerPosition);
                bool playerMovingAwayFromRobot =
                    (basePlayerPosition.x - transform.position.x) * playerVelocity.x > 0;

                if (playerMovingAwayFromRobot && distanceToRobot > 8f)
                {
                    // Player is far and moving away - target where they'll likely dash to escape
                    primaryTarget = playerVelocity.x > 0 ? rightDashTarget : leftDashTarget;
                    Debug.Log(
                        $"Player likely to dash away - targeting escape route: {primaryTarget}"
                    );
                }
                else
                {
                    // Player is close or not moving away - target center of potential positions
                    float centerX =
                        (leftDashTarget.x + rightDashTarget.x + currentTrajectory.x) / 3f;
                    primaryTarget = new Vector3(centerX, originalY, transform.position.z);
                    Debug.Log(
                        $"Player in combat range - targeting center of dash options: {primaryTarget}"
                    );
                }
            }
            else
            {
                // Standard prediction for grounded or slow-moving player
                float standardPredictionTime = jumpDuration * 0.6f;
                Vector3 predictedPosition =
                    basePlayerPosition + (playerVelocity * standardPredictionTime);
                primaryTarget = new Vector3(predictedPosition.x, originalY, transform.position.z);
                Debug.Log($"Standard prediction - targeting: {primaryTarget}");
            }
        }

        Vector3 committedTargetPosition = primaryTarget;

        Debug.Log(
            $"ADVANCED JUMP PREDICTION: Target: {committedTargetPosition}, Player velocity: {playerVelocity.x:F1} m/s, Is dashing: {playerIsDashing}"
        );

        // Face the player
        Vector2 direction = (committedTargetPosition - startPosition).normalized;
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(0.65f, 0.65f, 1);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-0.65f, 0.65f, 1);
        }

        // Play jump animation - this should stay active throughout the jump
        PlayAnimation(jump);

        // Animate the jump movement
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration && !isDead)
        {
            // If robot died during jump, immediately exit
            if (isDead)
            {
                Debug.Log("Robot died during jump - immediately stopping jump coroutine");
                isJumping = false;
                yield break;
            }

            // FIX: Don't update target position - stick to committed target
            // Calculate movement progress
            float progress = elapsedTime / jumpDuration;
            // Use easing for more natural jump movement
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            // Interpolate X position towards committed target
            float currentX = Mathf.Lerp(startPosition.x, committedTargetPosition.x, easedProgress);

            // Create a more realistic parabolic arc
            float arcHeight = 2f;
            // Use a parabolic function: -4 * (progress - 0.5)^2 + 1
            // This creates a smooth arc that peaks at the middle and gradually descends
            float normalizedProgress = progress * 2f - 1f; // Convert to -1 to 1 range
            float arc = arcHeight * (1f - normalizedProgress * normalizedProgress); // Parabolic curve
            float currentY = originalY + arc;

            // Set the new position
            Vector3 newPosition = new Vector3(currentX, currentY, transform.position.z);
            transform.position = newPosition;

            // Only ensure jump animation stays active if robot is still alive
            // This prevents overriding the death animation
            if (currentAnimation != jump && !isDead)
            {
                PlayAnimation(jump);
                Debug.Log("Re-applying jump animation to prevent override");
            }

            Debug.Log(
                $"Jump Progress: {progress:F2}, Position: ({currentX:F1}, {currentY:F1}), Arc: {arc:F1}, Target: Committed position X:{committedTargetPosition.x:F1}"
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // If robot died during jump, exit immediately without landing effects
        if (isDead)
        {
            Debug.Log("Robot died during jump - skipping landing sequence");
            isJumping = false;
            yield break;
        }

        // FIX: Land at the committed target position, not the current player position
        transform.position = committedTargetPosition;
        Debug.Log($"Jump completed - Landed at committed target: {transform.position}");

        // Instantiate landing effect only on landing
        if (jumpEffectPrefab != null)
        {
            Instantiate(jumpEffectPrefab, transform.position, Quaternion.identity);
            Debug.Log("Landing effect instantiated!");
        }

        // FIX: Deal damage based on the committed landing position, not robot's final position
        if (!isDead)
        {
            // Two-tier AOE damage system based on committed landing position
            // First check inner circle for full damage
            Collider2D[] innerTargets = Physics2D.OverlapCircleAll(
                committedTargetPosition, // Use committed position for damage calculation
                jumpAOERadius,
                playerLayerMask
            );
            // Then check outer circle for reduced damage
            Collider2D[] outerTargets = Physics2D.OverlapCircleAll(
                committedTargetPosition, // Use committed position for damage calculation
                jumpOuterAOERadius,
                playerLayerMask
            );

            bool playerHit = false;

            // Track which players got hit by inner AOE to avoid double damage
            HashSet<Collider2D> innerHitTargets = new HashSet<Collider2D>();

            // Inner AOE - Full damage
            foreach (Collider2D target in innerTargets)
            {
                if (target.CompareTag("Player"))
                {
                    PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.DealDamage((int)jumpDamage);
                        innerHitTargets.Add(target);
                        playerHit = true;
                        Debug.Log(
                            $"Robot jump INNER AOE hit player at committed landing position! Dealt {jumpDamage} damage! (Inner Radius: {jumpAOERadius})"
                        );
                    }
                }
            }

            // Outer AOE - Reduced damage (only for targets not hit by inner AOE)
            foreach (Collider2D target in outerTargets)
            {
                if (target.CompareTag("Player") && !innerHitTargets.Contains(target))
                {
                    PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.DealDamage((int)jumpOuterDamage);
                        playerHit = true;
                        Debug.Log(
                            $"Robot jump OUTER AOE hit player at committed landing position! Dealt {jumpOuterDamage} damage! (Outer Radius: {jumpOuterAOERadius})"
                        );
                    }
                }
            }

            if (!playerHit)
            {
                Debug.Log(
                    $"Jump attack missed - no players within AOE at committed landing position (Inner: {jumpAOERadius}, Outer: {jumpOuterAOERadius})"
                );
            }
        }
        else
        {
            Debug.Log("Robot died - no damage dealt");
        }

        // Brief pause after landing before switching animation
        yield return new WaitForSeconds(0.2f);

        // Wait for robot to be grounded before continuing (prevent air running)
        float groundCheckTimeout = 2f; // Maximum time to wait for grounding
        float groundCheckStartTime = Time.time;

        while (!isGrounded && !isDead && (Time.time - groundCheckStartTime) < groundCheckTimeout)
        {
            Debug.Log("Waiting for robot to be grounded after jump...");
            yield return new WaitForSeconds(0.1f);
        }

        if (!isGrounded && !isDead)
        {
            Debug.LogWarning("Robot never detected ground after jump - continuing anyway");
        }

        // Final check before completing jump - if robot died, don't continue
        if (isDead)
        {
            Debug.Log("Robot died after landing - stopping jump completion");
            isJumping = false;
            yield break;
        }

        // Now that we've landed, we can change animations
        isJumping = false;
        Debug.Log($"Jump attack completed - robot has landed! Grounded: {isGrounded}");

        // Set aggro after jump attack to continue pursuing player
        if (IsPlayerAlive())
        {
            isAggroed = true;
            aggroEndTime = Time.time + aggroTime;

            // Only switch to run animation if grounded, otherwise stay idle
            if (isGrounded)
            {
                PlayAnimation(GetRunAnimation());
            }
            else
            {
                Debug.Log("Robot not grounded after jump - staying idle");
                PlayAnimation(idle_1);
            }

            // Note: Boss music should already be playing from initial encounter start
        }
        else
        {
            PlayAnimation(idle_1);
        }

        // Update last jump attack time
        lastJumpAttackTime = Time.time;
    }

    IEnumerator PerformHealing()
    {
        if (isDead)
            yield break;

        isHealing = true;
        isInvincible = true;
        Debug.Log("Starting healing state - robot is now invincible!");

        // Start flashing effect
        StartCoroutine(FlashWhiteEffect());

        // Smooth transition: brief pause before main healing animation
        yield return new WaitForSeconds(0.2f);

        // Play healing animation
        PlayAnimation(flag_meeting);

        // Instantiate healing effect
        if (healingEffectPrefab != null)
        {
            GameObject healingEffect = Instantiate(
                healingEffectPrefab,
                transform.position,
                Quaternion.identity
            );
            Debug.Log("Healing effect instantiated!");

            // Optional: Destroy the effect after healing duration
            Destroy(healingEffect, healingDuration);
        }

        // Calculate healing intervals (heal every second)
        float healingInterval = 1f; // Heal every 1 second
        int totalHealingTicks = Mathf.FloorToInt(healingDuration / healingInterval);

        Debug.Log(
            $"HEALING PLAN: {totalHealingTicks} ticks over {healingDuration}s, {healingAmountPerTick} HP per tick = {totalHealingTicks * healingAmountPerTick} total HP | Current: {currentHealth}/{maxHealth}"
        );

        for (int i = 0; i < totalHealingTicks; i++)
        {
            // Wait for healing interval
            yield return new WaitForSeconds(healingInterval);

            // Check if robot died during healing (shouldn't happen due to invincibility)
            if (isDead)
            {
                Debug.Log("Robot died during healing - ending healing state");
                yield break;
            }

            // Restore health
            int healthBefore = currentHealth;
            int intendedHealing = healingAmountPerTick;
            int maxPossibleHealing = maxHealth - currentHealth;
            currentHealth = Mathf.Min(currentHealth + healingAmountPerTick, maxHealth);
            int actualHealing = currentHealth - healthBefore;

            // Update health bar
            if (enemyHealthBar != null)
            {
                enemyHealthBar.SetHealth(currentHealth);
            }

            Debug.Log(
                $"HEALING TICK {i + 1}: Intended: {intendedHealing} HP, Max Possible: {maxPossibleHealing} HP, Actual: {actualHealing} HP | Health: {healthBefore} → {currentHealth}/{maxHealth}"
            );

            // Stop healing if at max health
            if (currentHealth >= maxHealth)
            {
                Debug.Log("Robot reached max health - healing complete!");
                break;
            }
        }

        // Wait for any remaining time
        float remainingTime = healingDuration - (totalHealingTicks * healingInterval);
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // Check if robot died during healing (shouldn't happen due to invincibility)
        if (isDead)
        {
            Debug.Log("Robot died during healing - ending healing state");
            yield break;
        }

        // Stop flashing effect
        StopFlashing();

        // Smooth transition out of healing
        PlayAnimation(idle_1);
        yield return new WaitForSeconds(0.3f);

        // End healing state
        isHealing = false;
        isInvincible = false;
        Debug.Log("Healing completed - robot is no longer invincible!");

        // Update last healing time for cooldown
        lastHealingTime = Time.time;

        // Set aggro after healing to continue pursuing player
        if (IsPlayerAlive())
        {
            isAggroed = true;
            aggroEndTime = Time.time + aggroTime;

            // Note: Boss music should already be playing from initial encounter start
        }
    }

    IEnumerator FlashWhiteEffect()
    {
        if (skeletonAnimation == null)
            yield break;

        isFlashing = true;
        float flashSpeed = 0.12f; // Good visibility timing

        while (isFlashing && isHealing)
        {
            // Flash to pure white (bright invincible state)
            skeleton.SetColor(Color.white); // Pure white flash
            yield return new WaitForSeconds(flashSpeed);

            // Flash to much darker state for strong white contrast
            if (isFlashing && isHealing) // Check again in case healing ended
            {
                skeleton.SetColor(new Color(0.4f, 0.4f, 0.4f, 1f)); // Dark gray for contrast
                yield return new WaitForSeconds(flashSpeed);
            }
        }

        // Ensure we return to original color when done
        skeleton.SetColor(Color.white);
    }

    private void StopFlashing()
    {
        isFlashing = false;
        if (skeletonAnimation != null && skeleton != null)
        {
            skeleton.SetColor(Color.white);
        }
    }

    // Keep collision damage for when player touches robot
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        if (collision.gameObject.tag == "Player" && IsPlayerAlive())
        {
            // Don't deal collision damage during jump attacks - only jump landing damage should apply
            if (isJumping)
            {
                Debug.Log(
                    "Robot is jumping - skipping collision damage (jump landing damage will apply instead)"
                );
                return;
            }

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.DealDamage(damageAmount);
                Debug.Log($"Player touched robot - dealing {damageAmount} touch damage");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
        {
            if (isInvincible)
            {
                Debug.Log("Robot is invincible - no damage taken!");
            }
            return;
        }

        currentHealth -= damage;

        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetHealth(currentHealth);
        }

        // Set aggro state when taking damage
        if (IsPlayerAlive() && currentHealth > 0)
        {
            // Trigger boss music when encounter starts (only once)
            if (!bossEncounterStarted && AudioManager.instance != null)
            {
                AudioManager.instance.PlayBossBGM();
                bossEncounterStarted = true;
                Debug.Log("Boss took damage - boss encounter started!");
            }

            isAggroed = true;
            aggroEndTime = Time.time + aggroTime;
            PlayAnimation(GetRunAnimation());
            ChasePlayer();
            Debug.Log($"Robot took damage - aggroed for {aggroTime} seconds!");
        }

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        if (isDead)
            return;

        isDead = true;
        PlayAnimation(death);

        // Return to normal music when boss dies
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayNormalBGM();
            Debug.Log("Boss defeated - returning to normal music!");
        }

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Destroy(gameObject, 2.0f);
    }

    // Visual debug for ground check (visible in Scene view)
    private void OnDrawGizmosSelected()
    {
        // Draw ground check circle
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        else
        {
            // Show fallback position
            Vector2 checkPosition = new Vector2(transform.position.x, transform.position.y - 0.5f);
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
        }

        // Draw jump AOE radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, jumpAOERadius);

        // Draw outer AOE radius
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, jumpOuterAOERadius);

        // Draw jump range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, jumpRange);
    }
}
