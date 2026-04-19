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
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        UpdateHealthNumber();
        UpdateHealthBar();

        if (healthBar != null)
            healthBar.value = 1f;
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        UpdateHealthBar();
        UpdateHealthNumber();

        Vector2 knockbackDir = ((Vector2)transform.position - attackerPosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(InvincibilityFrames());
        StartCoroutine(KnockbackPause());

        if (currentHealth <= 0) Die();
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
        // hook up death screen later
    }
}