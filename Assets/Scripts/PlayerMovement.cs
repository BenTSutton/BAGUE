using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    //Basic Movement
    public float moveInput;
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;

    //Jumping on dis dick 
    public float jumpForce = 8f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    //Ground Check deez balls
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded;

    // STAMINA (In the Bedroom ;)
    public float maxStamina = 100f;
    public float stamina;
    public float sprintStaminaDrain = 20f;
    public float dashStaminaCost = 30f;
    public float staminaRegen = 15f;
    public float staminaRegenDelay = 1.5f;
    private float regenTimer = 0f;

    // Animation UwU
    public Animator animator;
    public SpriteRenderer sprite;
    

    // DASH 
    public float dashForce = 25f;
    public float dashTime = 0.2f;
    public float dashCooldown = 2f;

    //ATTACK!!!! 
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public int attackDamage = 1;
    public float attackCooldown = 0.5f;
    private float attackTimer = 0f;
    public float knockbackForce = 5f;

    // Smoothing player movement so it feels better. (Less clunky)
    public float acceleration = 13f;
    public float deceleration = 16f;
    public float airControlMultiplier = 0.85f;
    private float currentVelX = 0f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    private float coyoteCounter = 0f;
    private float jumpBufferCounter = 0f;

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
    public bool isKnockedBack = false;

    //parry
    // PARRY
    public float parryDuration = 0.5f;
    public float parryCooldown = 2.5f;
    public bool isParrying = false;
    private bool canParry = true;

    // RANGED ATTACK
    public GameObject bulletPrefab;
    public Transform firePoint; 
    public float rangedCooldown = 6f;
    private float rangedTimer = 0f;
    private bool isFiring = false;
    
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

        if (Input.GetMouseButtonDown(0) && attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
            StartCoroutine(AttackAnimation()); 
        }

        // RANGED ATTACK
        rangedTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && rangedTimer <= 0f && !isFiring)
        {
            StartCoroutine(RangedAttack());
            rangedTimer = rangedCooldown;
        }

        // Dash (From the incredibles)
        if (Input.GetKeyDown(KeyCode.LeftControl) && canDash && stamina >= dashStaminaCost)
        {
            stamina -= dashStaminaCost;

            regenTimer = staminaRegenDelay;

            StartCoroutine(Dash());
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
        if (Input.GetMouseButtonDown(1) && canParry)
        {
            StartCoroutine(Parry());
        }
    }

    void FixedUpdate()
    {
        // if (GameManager.Instance.currentState != GameState.Combat)
        // {
        //     return;
        // } 
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

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
            float targetVelX = moveInput * speed;
        
            // Reduce air control slightly
            float accelRate = isGrounded 
                ? (Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration)
                : (Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration) * airControlMultiplier;
    
            currentVelX = Mathf.MoveTowards(currentVelX, targetVelX, accelRate * Time.fixedDeltaTime * 10f);
            rb.linearVelocity = new Vector2(currentVelX, rb.linearVelocity.y);
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float dashDirection = moveInput;

        if (dashDirection == 0)
        {
            dashDirection = 1;
        }

        rb.linearVelocity = new Vector2(dashDirection * dashForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(dashTime);

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
    //Attack Stuff
    
    
    void Attack()
{
    float direction = facingRight ? 1f : -1f;
    Vector2 attackPos = new Vector2(
        transform.position.x + (Mathf.Abs(attackPoint.localPosition.x) * direction), //Confusing attack script stuff 
        transform.position.y + attackPoint.localPosition.y
    );

    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
        attackPos,
        attackRange, //Cofusing enemy detection stuff
        enemyLayer
    );

    Debug.Log("Enemies hit: " + hitEnemies.Length); 

    foreach (Collider2D enemy in hitEnemies)
    {
        enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);

        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 dir = (enemy.transform.position - transform.position).normalized;
            enemyRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            enemy.GetComponent<EnemyAI>()?.StartCoroutine("KnockbackPause"); //Knockback stuff 
        }
    }
}
           IEnumerator AttackAnimation()
        {
            animator.SetBool("isAttacking", true);
            yield return new WaitForSeconds(attackCooldown);
            animator.SetBool("isAttacking", false);
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
        IEnumerator Parry()
    {
        canParry = false;
        isParrying = true;
        Debug.Log("Parrying!");

        yield return new WaitForSeconds(parryDuration); // active parry the platypus window
        isParrying = false;

        yield return new WaitForSeconds(parryCooldown); // cooldown
        canParry = true;
    }
        IEnumerator RangedAttack()
    {
        isFiring = true;
        animator.SetTrigger("fire");
    
        yield return new WaitForSeconds(0.2f); // windup before bullet fires

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

        bullet.GetComponent<Bullet>().SetDirection(fireDirection);

        yield return new WaitForSeconds(0.3f); // finish animation (ok daddy)
        isFiring = false;
        
    }
}

// this is a mess 