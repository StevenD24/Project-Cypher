using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class RobotBoss : MonoBehaviour
{
    [Header("Movement")]
    public Transform pointA,
        pointB;
    public int Speed;
    private Vector3 currentTarget;

    [Header("Health & Combat")]
    public int currentHealth,
        maxHealth,
        damageAmount;
    public EnemyHealthBar enemyHealthBar;

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

    [Header("Player Detection & Chase")]
    public Transform player;
    public float detectingRange = 8f;
    public float timeBetweenAttacks = 1f;
    public float attackRange = 2f;

    // Spine components
    private SkeletonAnimation skeletonAnimation;
    private Spine.AnimationState spineAnimationState;
    private Spine.Skeleton skeleton;

    // State tracking
    private bool isDead = false;
    private bool isMoving = false;
    private bool isAttacking = false;
    private bool isChasing = false;
    private string currentAnimation = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize Spine2D components
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation != null)
        {
            spineAnimationState = skeletonAnimation.AnimationState;
            skeleton = skeletonAnimation.Skeleton;

            // Subscribe to animation complete events
            spineAnimationState.Complete += OnAnimationComplete;

            Debug.Log("Spine components initialized successfully");
        }
        else
        {
            Debug.LogError(
                "SkeletonAnimation component not found! Please add a SkeletonAnimation component to this GameObject."
            );
        }

        // Debug animation string values
        Debug.Log(
            $"Animation strings - idle_1: '{idle_1}', walk_1: '{walk_1}', run_1: '{run_1}', attack: '{attack}'"
        );

        // Initialize health
        currentHealth = maxHealth;
        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetMaxHealth(currentHealth);
        }

        // Set initial target - always start by going to Point A (commented out for chase-only behavior)
        if (pointA != null && pointB != null)
        {
            currentTarget = pointA.position;
            Debug.Log($"Robot starting position: {transform.position}");
            Debug.Log($"Point A: {pointA.position}, Point B: {pointB.position}");
            Debug.Log($"Initial target set to Point A: {currentTarget}");
        }
        else
        {
            Debug.LogWarning("Point A or Point B not assigned!");
        }

        // Play initial idle animation
        if (!string.IsNullOrEmpty(idle_1))
        {
            PlayAnimation(idle_1, true);
        }
        else
        {
            Debug.LogError("idle_1 animation string is empty! Please set it in the inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        // Check if player still exists
        if (player == null)
        {
            // Player was destroyed - stop all actions
            if (isAttacking || isChasing || isMoving)
            {
                Debug.Log("Player destroyed - stopping all robot actions and going idle");

                // Stop all current actions
                StopAllCoroutines();
                isAttacking = false;
                isChasing = false;
                isMoving = false;

                // Play idle animation
                PlayAnimation(idle_1, true);
            }
            return; // Exit update - no more actions needed
        }

        // Check if player is alive/active
        bool playerAlive = IsPlayerAlive();
        if (!playerAlive)
        {
            // Player is dead - stop all actions
            if (isAttacking || isChasing || isMoving)
            {
                Debug.Log("Player is dead - stopping all robot actions and going idle");

                // Stop all current actions
                StopAllCoroutines();
                isAttacking = false;
                isChasing = false;
                isMoving = false;

                // Play idle animation
                PlayAnimation(idle_1, true);
            }
            return; // Exit update - no more actions needed
        }

        // Simple chase logic - check if player exists and is in range
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        Debug.Log(
            $"Distance to player: {distanceToPlayer:F1}, isAttacking: {isAttacking}, playerAlive: {playerAlive}"
        );

        // If player is close enough to detect
        if (distanceToPlayer <= detectingRange)
        {
            // If close enough to attack AND not currently attacking
            if (distanceToPlayer <= attackRange && !isAttacking)
            {
                Debug.Log("ATTACKING PLAYER! Setting isAttacking to true immediately.");
                // Immediately set attacking state to prevent multiple attacks
                isAttacking = true;
                isMoving = false;
                isChasing = false;
                StartCoroutine(AttackAfterDelay());
            }
            // If not attacking, chase the player
            else if (!isAttacking)
            {
                Debug.Log("CHASING PLAYER!");

                // Immediately set chasing state and animation
                if (!isChasing)
                {
                    isChasing = true;
                    Debug.Log("Starting chase - immediately playing run animation");
                    PlayAnimation(run_1, true);
                }

                ChasePlayer();
            }
            else
            {
                Debug.Log("Player in range but robot is attacking - waiting for attack to finish");
            }
        }
        else
        {
            // Player too far - just idle
            if (isChasing || isMoving)
            {
                Debug.Log("Player too far - stopping chase and going idle");
                isMoving = false;
                isChasing = false;
                PlayAnimation(idle_1, true);
            }
        }

        // Update animations
        UpdateAnimations();
    }

    private bool IsPlayerAlive()
    {
        if (player == null)
            return false;

        // Check if player GameObject is active
        if (!player.gameObject.activeInHierarchy)
        {
            Debug.Log("Player GameObject is inactive");
            return false;
        }

        // Check if player has PlayerHealth component and is alive
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Assuming PlayerHealth has a way to check if player is alive
            // You might need to adjust this based on your PlayerHealth implementation
            try
            {
                bool alive = true; // Declare alive once here

                // Common ways to check if player is alive:
                // Option 1: Check if health > 0 (assuming there's a currentHealth field)
                var healthField = playerHealth.GetType().GetField("currentHealth");
                if (healthField != null)
                {
                    int currentHealth = (int)healthField.GetValue(playerHealth);
                    alive = currentHealth > 0;
                    Debug.Log($"Player health: {currentHealth}, alive: {alive}");
                    return alive;
                }

                // Option 2: Check if there's an isDead field
                var isDeadField = playerHealth.GetType().GetField("isDead");
                if (isDeadField != null)
                {
                    bool isDead = (bool)isDeadField.GetValue(playerHealth);
                    alive = !isDead;
                    Debug.Log($"Player isDead: {isDead}, alive: {alive}");
                    return alive;
                }

                // Option 3: Check if component is enabled (sometimes disabled when dead)
                alive = playerHealth.enabled;
                Debug.Log($"PlayerHealth enabled: {alive}");
                return alive;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not check player health: {e.Message}");
                // Fallback: assume alive if we can't check
                return true;
            }
        }

        // No PlayerHealth component found, assume alive if GameObject is active
        return true;
    }

    private void ChasePlayer()
    {
        // Double-check player still exists during chase
        if (player == null)
        {
            Debug.Log("Player destroyed during chase - stopping");
            isChasing = false;
            isMoving = false;
            return;
        }

        Vector3 previousPosition = transform.position;

        // Calculate direction to player (only X axis)
        float direction = player.transform.position.x - transform.position.x;

        // Set facing direction
        if (direction > 0)
        {
            SetFacingDirection(false); // Face right
        }
        else if (direction < 0)
        {
            SetFacingDirection(true); // Face left
        }

        // Move towards player with slower, more controlled speed
        Vector3 targetPosition = new Vector3(
            player.transform.position.x,
            transform.position.y,
            transform.position.z
        );

        // Use slower chase speed to better match animation
        float chaseSpeed = Speed + 1; // Reduced from +3 to +1
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            chaseSpeed * Time.deltaTime
        );

        // Always mark as moving when chasing
        isMoving = true;

        // Check actual movement for debugging
        float distanceMoved = Vector3.Distance(previousPosition, transform.position);

        Debug.Log(
            $"CHASE: moved {distanceMoved:F4}, speed: {chaseSpeed}, isMoving: {isMoving}, currentAnim: '{currentAnimation}'"
        );

        // Force animation update if not correct (with proper checks)
        if (!string.IsNullOrEmpty(run_1) && currentAnimation != run_1)
        {
            Debug.Log($"FORCING RUN ANIMATION! Current: '{currentAnimation}', Target: '{run_1}'");
            PlayAnimation(run_1, true);
        }
        else if (string.IsNullOrEmpty(run_1))
        {
            Debug.LogError("run_1 animation string is empty! Please set it in the inspector.");
        }
    }

    private void UpdateAnimations()
    {
        // Don't change animations while attacking
        if (isAttacking)
            return;

        // Simple immediate animation based on current state
        if (isMoving)
        {
            // Robot is moving - play appropriate movement animation
            if (isChasing)
            {
                // Chasing player - play run animation
                if (currentAnimation != run_1)
                {
                    Debug.Log("Robot is chasing - playing run animation");
                    PlayAnimation(run_1, true);
                }
            }
            else
            {
                // Regular movement - play walk animation
                if (currentAnimation != walk_1)
                {
                    Debug.Log("Robot is moving - playing walk animation");
                    PlayAnimation(walk_1, true);
                }
            }
        }
        else
        {
            // Robot is not moving - play idle animation
            if (currentAnimation != idle_1)
            {
                Debug.Log("Robot is idle - playing idle animation");
                PlayAnimation(idle_1, true);
            }
        }
    }

    private void SetFacingDirection(bool facingLeft)
    {
        if (skeleton != null)
        {
            skeleton.ScaleX = facingLeft ? -1 : 1;
        }
    }

    private void PlayAnimation(string animationName, bool loop)
    {
        // Add comprehensive checks
        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogWarning($"Cannot play animation - animationName is null or empty");
            return;
        }

        if (spineAnimationState == null)
        {
            Debug.LogWarning(
                $"Cannot play animation '{animationName}' - spineAnimationState is null. Make sure SkeletonAnimation component is attached."
            );
            return;
        }

        if (skeleton == null)
        {
            Debug.LogWarning($"Cannot play animation '{animationName}' - skeleton is null.");
            return;
        }

        // Check if the animation exists in the skeleton data
        try
        {
            var animation = spineAnimationState.Data.SkeletonData.FindAnimation(animationName);
            if (animation != null)
            {
                Debug.Log(
                    $"PLAYING ANIMATION: '{animationName}' (loop: {loop}, duration: {animation.Duration}s)"
                );
                spineAnimationState.SetAnimation(0, animationName, loop);
                currentAnimation = animationName;

                // Force immediate update
                spineAnimationState.Update(0);
                spineAnimationState.Apply(skeleton);
            }
            else
            {
                Debug.LogError(
                    $"Animation '{animationName}' not found in skeleton data for {gameObject.name}"
                );
                // List all available animations for debugging
                Debug.Log("Available animations:");
                var skeletonData = spineAnimationState.Data.SkeletonData;
                for (int i = 0; i < skeletonData.Animations.Count; i++)
                {
                    Debug.Log(
                        $"  - {skeletonData.Animations.Items[i].Name} (duration: {skeletonData.Animations.Items[i].Duration}s)"
                    );
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                $"Error playing animation '{animationName}' on {gameObject.name}: {e.Message}"
            );
        }
    }

    private void SafeAddAnimation(string animationName, bool loop, float delay)
    {
        if (spineAnimationState != null && !string.IsNullOrEmpty(animationName))
        {
            try
            {
                var animation = spineAnimationState.Data.SkeletonData.FindAnimation(animationName);
                if (animation != null)
                {
                    spineAnimationState.AddAnimation(0, animationName, loop, delay);
                }
                else
                {
                    Debug.LogWarning(
                        $"Animation '{animationName}' not found in skeleton data for {gameObject.name}"
                    );
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(
                    $"Error adding animation '{animationName}' on {gameObject.name}: {e.Message}"
                );
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        // Don't interrupt attack animations with hit animation
        if (!isAttacking)
        {
            // Play hit animation
            PlayAnimation(getHit, false);

            // Return to appropriate animation after hit
            SafeAddAnimation(isMoving ? walk_1 : idle_1, true, 0.5f);
        }
        else
        {
            Debug.Log(
                "Taking damage during attack - not playing hit animation to avoid interruption"
            );
        }

        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    // Keep the old method for compatibility with other scripts
    public void takeDamage()
    {
        TakeDamage(damageAmount);
    }

    public void Death()
    {
        if (isDead)
            return;

        isDead = true;
        Debug.Log("Robot Boss is dead");

        // Play death animation
        PlayAnimation(death, false);

        // Disable collider and movement
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        // Destroy after animation completes
        Destroy(gameObject, 2.0f); // Increased time for death animation
    }

    public void PlayAttackAnimation()
    {
        if (!isDead && !isAttacking)
        {
            isAttacking = true;
            Debug.Log("Robot starting attack - movement should stop");

            // Cancel any pending EndAttack calls to prevent conflicts
            CancelInvoke("EndAttack");
            StopCoroutine("WaitForAttackComplete");

            PlayAnimation(skill_1, false);

            // Start coroutine to wait for actual animation duration
            StartCoroutine(WaitForAttackComplete(attack));
        }
        else if (isAttacking)
        {
            Debug.Log("Attack already in progress - ignoring new attack trigger");
        }
    }

    public void PlaySkillAnimation(int skillNumber)
    {
        if (!isDead && !isAttacking)
        {
            isAttacking = true;
            Debug.Log($"Robot starting skill {skillNumber} - movement should stop");

            // Cancel any pending EndAttack calls to prevent conflicts
            CancelInvoke("EndAttack");
            StopCoroutine("WaitForAttackComplete");

            string skillAnim = skillNumber == 1 ? skill_1 : skill_2;
            PlayAnimation(skillAnim, false);

            // Start coroutine to wait for actual animation duration
            StartCoroutine(WaitForAttackComplete(skillAnim));
        }
        else if (isAttacking)
        {
            Debug.Log("Attack already in progress - ignoring new skill trigger");
        }
    }

    private System.Collections.IEnumerator WaitForAttackComplete(string animationName)
    {
        // Get the actual duration of the animation from Spine (for debugging)
        float spineDuration = 2.0f; // Default fallback

        if (spineAnimationState != null && !string.IsNullOrEmpty(animationName))
        {
            try
            {
                var animation = spineAnimationState.Data.SkeletonData.FindAnimation(animationName);
                if (animation != null)
                {
                    spineDuration = animation.Duration;
                    Debug.Log($"Spine reports {animationName} duration: {spineDuration}s");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not get animation duration: {e.Message}");
            }
        }

        // Use fixed durations since Spine is reporting incorrect values
        float actualDuration;
        if (animationName == attack)
        {
            actualDuration = 1.5f; // Fixed duration for attack - adjust this to match your animation
            Debug.Log(
                $"Using FIXED duration for attack: {actualDuration}s (Spine incorrectly reports: {spineDuration}s)"
            );
        }
        else if (animationName == skill_1 || animationName == skill_2)
        {
            actualDuration = 2.5f; // Fixed duration for skills
            Debug.Log(
                $"Using FIXED duration for skill: {actualDuration}s (Spine incorrectly reports: {spineDuration}s)"
            );
        }
        else
        {
            actualDuration = spineDuration; // Use Spine duration for other animations
            Debug.Log($"Using Spine duration for {animationName}: {actualDuration}s");
        }

        // Wait for the animation to complete
        yield return new WaitForSeconds(actualDuration);

        Debug.Log($"Animation {animationName} complete after {actualDuration}s");
        EndAttack();
    }

    private void EndAttack()
    {
        if (!isAttacking)
            return; // Prevent multiple calls

        Debug.Log("Ending attack - resuming movement");
        isAttacking = false;

        // Cancel any pending EndAttack timer
        CancelInvoke("EndAttack");

        // Always return to walk animation after attack
        PlayAnimation(walk_1, true);
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        // Check if the completed animation was an attack or skill
        string completedAnimation = trackEntry.Animation.Name;
        float duration = trackEntry.Animation.Duration;
        Debug.Log(
            $"Animation completed: {completedAnimation}, Duration: {duration}s, Track: {trackEntry.TrackIndex}"
        );

        if (
            completedAnimation == attack
            || completedAnimation == skill_1
            || completedAnimation == skill_2
        )
        {
            Debug.Log("Attack/Skill animation completed - ending attack state");
            EndAttack();
        }
        else
        {
            Debug.Log($"Non-attack animation completed: {completedAnimation}");
        }
    }

    // Damage player on contact
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        if (collision.gameObject.tag == "Player")
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.DealDamage();
                // Only play attack animation if not already attacking
                if (!isAttacking)
                {
                    Debug.Log("Player collision detected - triggering attack");
                    PlayAttackAnimation();
                }
                else
                {
                    Debug.Log("Player collision detected but already attacking - skipping");
                }
            }
        }
    }

    IEnumerator AttackAfterDelay()
    {
        Debug.Log($"AttackAfterDelay started - isAttacking: {isAttacking}");

        // Double-check we're in attacking state
        if (!isAttacking)
        {
            Debug.LogWarning(
                "AttackAfterDelay called but isAttacking is false! This shouldn't happen."
            );
            yield break;
        }

        Debug.Log("Robot attacking player!");

        // Play random attack animation
        string[] attacks = { attack, skill_1, skill_2 };
        string selectedAttack = attacks[Random.Range(0, attacks.Length)];
        Debug.Log($"Selected attack: {selectedAttack}");

        PlayAnimation(selectedAttack, false);

        // Use timeBetweenAttacks as the total attack duration
        Debug.Log(
            $"Attack will last for {timeBetweenAttacks} seconds total (includes animation + cooldown)"
        );

        // Wait for attack duration, but check if player is destroyed during wait
        float timeElapsed = 0f;
        while (timeElapsed < timeBetweenAttacks)
        {
            // Check if player was destroyed during attack
            if (player == null)
            {
                Debug.Log("Player destroyed during attack - stopping attack");
                isAttacking = false;
                PlayAnimation(idle_1, true);
                yield break;
            }

            yield return null;
            timeElapsed += Time.deltaTime;
        }

        Debug.Log("Attack timer finished - setting isAttacking to false");
        isAttacking = false;
        Debug.Log("Attack complete - resuming behavior");

        // Return to appropriate animation (check player still exists)
        if (player != null)
        {
            if (isChasing)
            {
                PlayAnimation(run_1, true);
            }
            else
            {
                PlayAnimation(walk_1, true);
            }
        }
        else
        {
            PlayAnimation(idle_1, true);
        }
    }
}

// Enemy bullet class for boss projectiles
public class EnemyBullet : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.DealDamage();
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
