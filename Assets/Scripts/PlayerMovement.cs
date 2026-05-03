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

    // Animation 
    public Animator animator;
    public SpriteRenderer sprite;


    // DASH 
    public float dashForce = 25f;
    public float dashTime = 0.2f;
    public float dashCooldown = 2f;

    // U&I Date? (UI)
    public Slider staminaBar;
    public Image staminaFill;
    public TMP_Text staminaText;
    private float displayedStamina;
    public float staminaSmoothSpeed = 6f;

    private bool isDashing = false;
    private bool canDash = true;

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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
            //Sprite flipping 
        if (moveInput > 0)
        {
            sprite.flipX = true;
        }
        else if (moveInput < 0)
        {
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
}
// this is a mess 