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

    // Spine components
    private SkeletonAnimation skeletonAnimation;
    private Spine.AnimationState spineAnimationState;
    private Spine.Skeleton skeleton;

    // State tracking
    private bool isDead = false;
    private bool isMoving = false;
    private bool wasMovingLastFrame = false;
    private bool isAttacking = false;
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
        }

        // Initialize health
        currentHealth = maxHealth;
        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetMaxHealth(currentHealth);
        }

        // Set initial target - always start by going to Point A
        if (pointA != null && pointB != null)
        {
            currentTarget = pointA.position;
            Debug.Log($"Robot starting position: {transform.position}");
            Debug.Log($"Point A: {pointA.position}, Point B: {pointB.position}");
            Debug.Log($"Initial target set to Point A: {currentTarget}");
        }
        else
        {
            Debug.LogError("Point A or Point B not assigned!");
        }

        PlayAnimation(idle_1, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        // Handle movement and direction
        HandleMovement();

        // Update animations based on movement state
        UpdateAnimations();
    }

    private void HandleMovement()
    {
        // Don't move while attacking
        if (isAttacking)
        {
            isMoving = false;
            return;
        }

        Vector3 previousPosition = transform.position;

        if (transform.position == pointA.position)
        {
            currentTarget = pointB.position;
            SetFacingDirection(false); // Face right (equivalent to sr.flipX = false)
        }
        else if (transform.position == pointB.position)
        {
            currentTarget = pointA.position;
            SetFacingDirection(true); // Face left (equivalent to sr.flipX = true)
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            Speed * Time.deltaTime
        );

        // Check if actually moving (needed for animations)
        float distanceMoved = Vector3.Distance(previousPosition, transform.position);
        isMoving = distanceMoved > 0.001f;
    }

    private void UpdateAnimations()
    {
        // Don't change animations while attacking
        if (isAttacking)
            return;

        // Only change animations when movement state changes
        if (isMoving && !wasMovingLastFrame)
        {
            // Just started moving - play walking animation (looped for continuous movement)
            Debug.Log("Robot started moving - switching to walk animation");
            PlayAnimation(walk_1, true);
        }
        else if (!isMoving && wasMovingLastFrame)
        {
            // Just stopped moving - play idle animation
            Debug.Log("Robot stopped moving - switching to idle animation");
            PlayAnimation(idle_1, true);
        }

        // Update the previous state for next frame
        wasMovingLastFrame = isMoving;
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
        if (spineAnimationState != null && !string.IsNullOrEmpty(animationName))
        {
            // Check if the animation exists in the skeleton data
            try
            {
                var animation = spineAnimationState.Data.SkeletonData.FindAnimation(animationName);
                if (animation != null)
                {
                    Debug.Log(
                        $"Playing animation: '{animationName}' (loop: {loop}, duration: {animation.Duration}s)"
                    );
                    spineAnimationState.SetAnimation(0, animationName, loop);
                    currentAnimation = animationName;
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
        else
        {
            Debug.LogWarning(
                $"Cannot play animation - spineAnimationState is null or animationName is empty: '{animationName}'"
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
