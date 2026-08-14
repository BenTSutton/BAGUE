using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public float knockbackForce = 5f;
    public float invincibilityDuration = 0.5f;

    // UI
    public Slider healthBar;
    public Image healthFill;
    public TMP_Text healthText;

    private bool isInvincible = false;
    private Rigidbody2D rb;

    void Start()
    {
        //Adjust max health depending on room levels
        KitchenRoom kitchenRoom = RunManager.Instance.GetRoomData<KitchenRoom>();
        MedRoom medRoom = RunManager.Instance.GetRoomData<MedRoom>();

        if (RunManager.Instance.IsRoomOperational<KitchenRoom>())
        {
            maxHealth += kitchenRoom.GetPlayerMaxHealthBonus(
                RunManager.Instance.GetRoomLevel<KitchenRoom>());
        }

        if (RunManager.Instance.IsRoomOperational<MedRoom>())
        {
            maxHealth += medRoom.GetMaxHealthBonus(
                RunManager.Instance.GetRoomLevel<MedRoom>());
        }

        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        healthText = GameObject.Find("HelthText").GetComponent<TMP_Text>();

        UpdateHealthNumber();
        UpdateHealthBar();

        if (healthBar != null)
            healthBar.value = 1f;

        //Regeneration from Med room 
        int regenerationAmount = 0;

        if (RunManager.Instance.IsRoomOperational<MedRoom>())
        {
            regenerationAmount = medRoom.GetRegenerationAmount(
                RunManager.Instance.GetRoomLevel<MedRoom>());
        }

        if (regenerationAmount > 0)
        {
            StartCoroutine(RegenerateHealth(
                regenerationAmount,
                medRoom.GetRegenerationInterval()));
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        //Modify healing from kitchen room
        KitchenRoom kitchenRoom = RunManager.Instance.GetRoomData<KitchenRoom>();

        if (RunManager.Instance.IsRoomOperational<KitchenRoom>())
        {
            amount = kitchenRoom.ModifyPlayerHealing(
                amount,
                RunManager.Instance.GetRoomLevel<KitchenRoom>());
        }
        else
        {
            amount = Mathf.FloorToInt(amount * 0.75f);
        }

        if (amount <= 0)
            return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (currentHealth > previousHealth)
            SFXManager.Instance?.PlayPlayerHealed(transform.position);

        UpdateHealthBar();
        UpdateHealthNumber();
    }

    private IEnumerator RegenerateHealth(int amount, float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (RunManager.Instance.IsRoomOperational<MedRoom>())
            {
                Heal(amount);
            }
        }
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (isInvincible || damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        UpdateHealthBar();
        UpdateHealthNumber();

        CombatFeedback.Instance?.PlayPlayerDamaged();

        Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        StartCoroutine(InvincibilityFrames());
        StartCoroutine(KnockbackPause());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator KnockbackPause()
    {
        GetComponent<PlayerMovement>().isKnockedBack = true;
        yield return new WaitForSeconds(0.2f);
        GetComponent<PlayerMovement>().isKnockedBack = false;
    }

    void UpdateHealthBar()
    {
        if (healthBar == null) return;

        healthBar.value = (float)currentHealth / maxHealth;

        if (healthFill == null) return;

        float pct = (float)currentHealth / maxHealth;
        if (pct > 0.6f)       healthFill.color = Color.green;
        else if (pct > 0.3f)  healthFill.color = Color.yellow;
        else                   healthFill.color = Color.red;
    }
        void UpdateHealthNumber()
    {
        if (healthBar != null)
            healthBar.value = (float)currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = "HP: " + currentHealth + " / " + maxHealth; 
    }

    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player died!");
        SFXManager.Instance?.PlayPlayerDie(transform.position);
        GameManager.Instance.LoseCombat();
    }

    public void TakeDamage(int damage, Transform attacker) 
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();

        // If parrying, knock the enemy back instead
        if (movement != null && movement.isParrying)
        {
            Debug.Log("Parried!");

            CombatFeedback.Instance?.PlayParry();

            EnemyAI enemyAI = attacker.GetComponent<EnemyAI>();

            // Stun first because Stun() clears the enemy's velocity.
            if (enemyAI != null)
                enemyAI.Stun(movement.parryStunDuration);

            Rigidbody2D enemyRb = attacker.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                Vector2 knockbackDirection = (attacker.position - transform.position).normalized;

                enemyRb.linearVelocity = Vector2.zero;

                enemyRb.AddForce(knockbackDirection * movement.parryKnockback, ForceMode2D.Impulse);
            }

            return;
        }

        if (isInvincible) return;

        currentHealth -= damage;
        UpdateHealthBar();
        UpdateHealthNumber();

        CombatFeedback.Instance?.PlayPlayerDamaged();

        Vector2 knockbackDir2 = ((Vector2)transform.position - (Vector2)attacker.position).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDir2 * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(InvincibilityFrames());
        StartCoroutine(KnockbackPause());

        if (currentHealth <= 0) Die();
    }
}
