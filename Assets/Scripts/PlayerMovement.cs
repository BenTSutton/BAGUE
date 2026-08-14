using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public enum PlayerActionState
{
    Free,
    LightAttack,
    HeavyAttack,
    RangedAttack,
    Parry,
    Dashing,
    Knockback
}
public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    [Header("Movement")]
    //Basic Movement
    public float moveInput;
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;

    //Jumping on dis dick 
    public float jumpForce = 8f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    // Smoothing player movement so it feels better. (Less clunky)
    public float acceleration = 13f;
    public float deceleration = 16f;
    public float airControlMultiplier = 0.85f;
    private float currentVelX = 0f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    private float coyoteCounter = 0f;
    private float jumpBufferCounter = 0f;

    [Header("Ground check")]
    //Ground Check deez balls
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded;

    [Header("Stamina")]
    // STAMINA (In the Bedroom ;)
    public float maxStamina = 100f;
    public float stamina;
    public float sprintStaminaDrain = 20f;
    public float dashStaminaCost = 30f;
    public float staminaRegen = 15f;
    public float staminaRegenDelay = 1.5f;
    private float regenTimer = 0f;

    [Header("Animation")]
    // Animation UwU
    public Animator animator;
    public SpriteRenderer sprite;
    
    [Header("Dash")]
    // DASH 
    public float dashForce = 25f;
    public float dashTime = 0.2f;
    public float dashCooldown = 2f;

    [Header("Light Attack")]
    //ATTACK!!!! 
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public int attackDamage = 1;
    public float attackCooldown = 0.5f;
    private float attackTimer = 0f;
    public float knockbackForce = 5f;
    public float knockbackEnemyPauseTime = 0.2f;
    private bool isAttacking = false;

    [Header("Heavy Attack")]
    //Heavy Attack
    [SerializeField] private int heavyDamage = 3;
    [SerializeField] private float heavyKnockback = 25f;
    [SerializeField] private float heavyStaminaCost = 25f;
    [SerializeField] private float heavyKnockbackEnemyPauseTime = 0.45f;

    [Header("UI")]
    // U&I Date? (UI)
    public Slider staminaBar;
    public Image staminaFill;
    public TMP_Text staminaText;
    private float displayedStamina;
    public float staminaSmoothSpeed = 6f;
    //misc movment 
    private bool isDashing = false;
    private bool canDash = true;
    private bool facingRight = true;
    private bool groundStateInitialized;
    private float footstepSoundTimer;
    private const float FootstepSoundInterval = 0.38f;

    [Header("Parry")]
    //parry
    // PARRY
    public float parryDuration = 0.5f;
    public float parryCooldown = 2.5f;
    public float parryKnockback = 40f;
    public float parryStunDuration = 1f;
    public bool isParrying = false;
    private bool canParry = true;

    [Header("Gun")]
    // RANGED ATTACK
    public GameObject bulletPrefab;
    public Transform firePoint; 
    public float rangedCooldown = 6f;
    private float rangedTimer = 0f;
    private bool hasFiredGunSinceReloaded = false;
    private bool isFiring = false;

    [Header("Misc")]
    public PlayerActionState actionState = PlayerActionState.Free;
    public bool isKnockedBack = false;

    //Must be free to do an action
    private bool CanStartAction => actionState == PlayerActionState.Free;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        stamina = maxStamina;
        displayedStamina = maxStamina;

        animator = GetComponent<Animator>();  //Sprite Animation
        sprite = GetComponent<SpriteRenderer>(); //Sprite flip 
    }

    void Update()
    {
        // If the GameManager is not in the combat state then kill speed to a standstill and dont run anything in update
        if (GameManager.Instance.currentState != GameState.Combat)
        {
            animator.SetBool("isRunning", false);
            rb.linearVelocity = Vector2.zero;
            moveInput = 0;
            return;
        } 
        moveInput = Input.GetAxis("Horizontal");

        // Jump 
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // Jump buffering
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Jump (now uses both buffers)
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
            //Sprite flipping 
        if (moveInput > 0 && !facingRight)
        {
            facingRight = true;
            sprite.flipX = false;
        }
        else if (moveInput < 0 && facingRight)
        {
            facingRight = false;
            sprite.flipX = true;
        }   
        //Jump fixing, to try and make less floaty. (couldn't find a good guide so.. Chat GPT wrote it) 
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        //ATTACK 
        attackTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            if (CanStartAction && attackTimer <= 0f)
                StartLightAttack();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (CanStartAction && stamina >= heavyStaminaCost)
                StartHeavyAttack();
            else
                SFXManager.Instance?.PlayPlayerCooldownFeedback(transform.position);
        }

        // RANGED ATTACK
        rangedTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (rangedTimer <= 0f && CanStartAction)
                StartRangedAttack();
            else
                SFXManager.Instance?.PlayPlayerCooldownFeedback(transform.position);
        }

        // Dash (From the incredibles)
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (CanStartAction && stamina >= dashStaminaCost && canDash)
            {
                stamina -= dashStaminaCost;

                regenTimer = staminaRegenDelay;
                Debug.Log("Should start Dash");
                StartCoroutine(Dash());
            }
            else
            {
                SFXManager.Instance?.PlayPlayerCooldownFeedback(transform.position);
            }
        }

        // Smooth stamina bar
        displayedStamina = Mathf.Lerp(displayedStamina, stamina, Time.deltaTime * staminaSmoothSpeed);
        staminaBar.value = displayedStamina / maxStamina;

        //Colour Changing Stamina Bar & Alfie Sucks ;)
        float staminaPercent = stamina / maxStamina;

        if (staminaPercent > 0.6f)
        {
            staminaFill.color = Color.green;
        }
        else if (staminaPercent > 0.3f)
        {
            staminaFill.color = Color.yellow;
        }
        else
        {
            staminaFill.color = Color.red;
        }
            staminaText.text = Mathf.RoundToInt(stamina) + " / " + maxStamina;

        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        if (moveInput != 0)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
        // PARRY BIT
        if (Input.GetMouseButtonDown(1))
        {
            if (CanStartAction && canParry)
                StartParry();
        }

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("horizontalSpeed", rb.linearVelocity.x);
        animator.SetFloat("verticalSpeed", rb.linearVelocity.y);

        if(!hasFiredGunSinceReloaded && rangedTimer <= 0f)
        {
            SFXManager.Instance?.PlayPlayerGunReload(transform.position);
            hasFiredGunSinceReloaded = true;
        }
    }

    void FixedUpdate()
    {
        // if (GameManager.Instance.currentState != GameState.Combat)
        // {
        //     return;
        // } 
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (groundStateInitialized && isGrounded && !wasGrounded)
            SFXManager.Instance?.PlayPlayerLand(transform.position);

        groundStateInitialized = true;

        if (isDashing) return;

        float speed = moveSpeed;

        // Sprint
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
                speed = sprintSpeed;
                stamina -= sprintStaminaDrain * Time.fixedDeltaTime;

            regenTimer = staminaRegenDelay;
        }

        // Regen delay stuff 
        if (regenTimer > 0)
            {
                regenTimer -= Time.fixedDeltaTime;
            }
        else
            {
                stamina += staminaRegen * Time.fixedDeltaTime;
            }

            // CLAMP STAMINA HERE
            stamina = Mathf.Clamp(stamina, 0, maxStamina);

            if (!isKnockedBack)
        {
            float targetVelX = moveInput * speed * GetActionMovementMultiplier();
        
            // Reduce air control slightly
            float accelRate = isGrounded 
                ? (Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration)
                : (Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration) * airControlMultiplier;
    
            currentVelX = Mathf.MoveTowards(currentVelX, targetVelX, accelRate * Time.fixedDeltaTime * 10f);
            rb.linearVelocity = new Vector2(currentVelX, rb.linearVelocity.y);
        }

        UpdateFootstepSound();
    }

    private void UpdateFootstepSound()
    {
        bool isWalking = isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (!isWalking)
        {
            footstepSoundTimer = 0f;
            return;
        }

        footstepSoundTimer -= Time.fixedDeltaTime;

        if (footstepSoundTimer <= 0f)
        {
            SFXManager.Instance?.PlayPlayerFootstep(transform.position);
            footstepSoundTimer = FootstepSoundInterval;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        actionState = PlayerActionState.Dashing;
        SFXManager.Instance?.PlayPlayerDash(transform.position);

        float dashDirection = moveInput;

        if (dashDirection == 0)
        {
            dashDirection = 1;
        }

        Debug.Log("Should now actually dash");
        rb.linearVelocity = new Vector2(dashDirection * dashForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        currentVelX = rb.linearVelocity.x;
        actionState = PlayerActionState.Free;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
    //Attack Stuff
    
    
    public void MeleeAttack(int damage, float knockback, float range, float knockbackPauseTime)
    {
        float direction = facingRight ? 1f : -1f;
        Vector2 attackPos = new Vector2(
            transform.position.x + (Mathf.Abs(attackPoint.localPosition.x) * direction), //Confusing attack script stuff 
            transform.position.y + attackPoint.localPosition.y
        );

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPos,
            range, //Cofusing enemy detection stuff
            enemyLayer
        );

        Debug.Log("Enemies hit: " + hitEnemies.Length); 

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null)
                continue;

            enemyHealth.TakeDamage(damage, EnemyHitType.Melee);

            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 dir = (enemy.transform.position - transform.position).normalized;
                enemyRb.AddForce(dir * knockback, ForceMode2D.Impulse);
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                enemyAI.StartCoroutine(enemyAI.KnockbackPause(knockbackPauseTime)); //Knockback stuff 
            }
        }
    }

    private void StartLightAttack()
    {
        actionState = PlayerActionState.LightAttack;
        attackTimer = attackCooldown;

        CombatFeedback.Instance?.PlayAttackSound(EnemyHitType.Melee);

        animator.SetTrigger("lightAttack");
    }

    private void StartHeavyAttack()
    {
        stamina -= heavyStaminaCost;
        regenTimer = staminaRegenDelay;

        actionState = PlayerActionState.HeavyAttack;


        animator.SetTrigger("heavyAttack");
    }
    
    public void LightHit()
    {
        MeleeAttack(attackDamage, knockbackForce, attackRange, knockbackEnemyPauseTime);
    }

    public void LightAttackSound()
    {
        SFXManager.Instance?.PlayPlayerHeavyAttack(transform.position);
    }

    public void HeavyAttackSound()
    {
        SFXManager.Instance?.PlayPlayerHeavyAttack(transform.position);
    }

    public void HeavyHit()
    {
        MeleeAttack(heavyDamage, heavyKnockback, attackRange * 1.2f, heavyKnockbackEnemyPauseTime);
    }

    public void FinishAttack()
    {
        actionState = PlayerActionState.Free;
    }

    void OnDrawGizmos()
    {   
        if (attackPoint == null) return;
        float direction = facingRight ? 1f : -1f;
        Vector2 attackPos = new Vector2(
        transform.position.x + (Mathf.Abs(attackPoint.localPosition.x) * direction),
        transform.position.y + attackPoint.localPosition.y
        );
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }

    //this is parry, but might be in the wrong place.
    IEnumerator ParryCooldown()
    {
        yield return new WaitForSeconds(parryCooldown); // cooldown
        canParry = true;
    }

    void StartParry()
    {
        canParry = false;
        SFXManager.Instance?.PlayPlayerEnterParry(transform.position);
        animator.SetTrigger("parry");
        actionState = PlayerActionState.Parry;
        Debug.Log("Parrying!");
    }

    public void EnableParrying()
    {
        isParrying = true;
    }

    public void FinishParry()
    {
        actionState = PlayerActionState.Free;
        isParrying = false;
        StartCoroutine(ParryCooldown());
        canParry = true;
    }

    private void StartRangedAttack()
    {
        if (!CanStartAction || rangedTimer > 0f)
        return;

        actionState = PlayerActionState.RangedAttack;
        rangedTimer = rangedCooldown;
        animator.SetTrigger("fire");
    }

    public void FireGun()
    {
        // Fire direction (Backshots.. from the front)
        float direction = facingRight ? 1f : -1f;
        Vector2 fireDirection = facingRight ? Vector2.right : Vector2.left;

            // Mirror firePoint like attackPoint does, since sprite.flipX doesn't move the transform
            Vector3 spawnPos = new Vector3(
            transform.position.x + (Mathf.Abs(firePoint.localPosition.x) * direction),
            transform.position.y + firePoint.localPosition.y,
            firePoint.position.z
        );

        GameObject bullet = Instantiate(
            bulletPrefab,
            spawnPos,
            Quaternion.identity
        );

        CombatFeedback.Instance?.PlayAttackSound(EnemyHitType.Gun);

        bullet.GetComponent<Bullet>().SetDirection(fireDirection);

        rangedTimer = rangedCooldown;
        hasFiredGunSinceReloaded = false;
    }

    public void FinishGunAttack()
    {
        actionState = PlayerActionState.Free;
    }

    //Player can still move during anims
    private float GetActionMovementMultiplier()
    {
        switch (actionState)
        {
            case PlayerActionState.LightAttack:
                return 0.55f;

            case PlayerActionState.HeavyAttack:
                return 0.1f;

            case PlayerActionState.RangedAttack:
                return 0.45f;

            case PlayerActionState.Parry:
                return 0.25f;

            case PlayerActionState.Knockback:
                return 0f;

            default:
                return 1f;
        }
    }
}

// this is a mess
