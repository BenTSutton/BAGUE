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
        moveInput = Input.GetAxis("Horizontal");

        // Jump 
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
            //Sprite flipping 
        if (moveInput > 0 && !facingRight)
        {
            facingRight = true;
            sprite.flipX = true;
        }
        else if (moveInput < 0 && facingRight)
        {
            facingRight = false;
            sprite.flipX = false;
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
    }

    void FixedUpdate()
    {
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
                rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
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
}
// this is a mess 