using System.Collections;
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
    private bool isAttacking = false;
    private bool isAggroed = false; // Robot is aggroed and will chase regardless of range
    private float aggroEndTime = 0f; // When aggro state should end
    private GameObject player;
    private string currentAnimation = "";
    private float lastAttackTime = -999f; // Track when last attack happened

    void Start()
    {
        // Initialize Spine2D components
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation != null)
        {
            spineAnimationState = skeletonAnimation.AnimationState;
            skeleton = skeletonAnimation.Skeleton;
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

        CalculateDistance();
    }

    private bool IsPlayerAlive()
    {
        return player != null && player.activeInHierarchy;
    }

    private string GetRunAnimation()
    {
        // Use tired walk animation when health is below 30%
        float healthPercentage = (float)currentHealth / maxHealth;
        return healthPercentage < 0.3f ? walk_2 : run_1;
    }

    private int GetCurrentSpeed()
    {
        // Use speed of 1 when health is below 30%
        float healthPercentage = (float)currentHealth / maxHealth;
        return healthPercentage < 0.3f ? 1 : speed;
    }

    private void CalculateDistance()
    {
        // If currently attacking, let the attack finish even if player dies
        if (isAttacking)
        {
            Debug.Log("Currently attacking - letting attack finish regardless of player state");
            return;
        }

        // Check if player exists and is alive before doing anything
        if (!IsPlayerAlive())
        {
            Debug.Log("Player is null or dead - stopping all actions");
            PlayAnimation(idle_1);
            return;
        }

        // Check if aggro should expire
        if (isAggroed && Time.time > aggroEndTime)
        {
            isAggroed = false;
            Debug.Log("Aggro expired - returning to normal behavior");
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);
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
                    PlayAnimation(idle_1);
                }
                else
                {
                    Debug.Log("Chasing player!");
                    // Play run animation and move towards player
                    PlayAnimation(GetRunAnimation());
                    ChasePlayer();
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

    private void ChasePlayer()
    {
        // Check if player exists and is alive before chasing
        if (!IsPlayerAlive())
        {
            Debug.Log("Player is null or dead - cannot chase");
            return;
        }

        Vector2 playerPosition = new Vector2(player.transform.position.x, transform.position.y);
        Vector2 direction = (playerPosition - (Vector2)transform.position).normalized;

        Debug.Log(
            $"Chasing: Robot at {transform.position.x:F1}, Player at {player.transform.position.x:F1}, Direction: {direction.x:F2}"
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
                    playerHealth.DealDamage();
                    Debug.Log("Robot dealt damage to player!");
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
        PlayAnimation(idle_1);

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

    // Keep collision damage for when player touches robot
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        if (collision.gameObject.tag == "Player" && IsPlayerAlive())
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.DealDamage();
                Debug.Log("Player touched robot - dealing touch damage");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetHealth(currentHealth);
        }

        // Set aggro state when taking damage
        if (IsPlayerAlive() && currentHealth > 0)
        {
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

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Destroy(gameObject, 2.0f);
    }
}
