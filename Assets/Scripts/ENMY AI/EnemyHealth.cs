using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int health;
    private bool isDead;

    public void Initialize(int maxHealth)
    {
        //Health never equals 0 when spawning
        health = Mathf.Max(1, maxHealth);
    }

    public void TakeDamage(int damage, EnemyHitType hitType = EnemyHitType.Environment)
    {
        if (isDead || damage <= 0)
            return;

        health -= damage;

        bool killed = health <= 0;

        GetComponent<EnemyHitFeedback>()?.PlayHit(damage, hitType, killed);

        if (killed)
            Die();
    }

    void Die() //What happens when die. 
    {
        isDead = true;

        // Med room healing when enemy is killed
        int healing = 0;

        if (RunManager.Instance.IsRoomOperational<MedRoom>())
        {
            MedRoom medRoom = RunManager.Instance.GetRoomData<MedRoom>();

            healing = medRoom.GetBoarderKillHealing(
                RunManager.Instance.GetRoomLevel<MedRoom>());
        }

        // Heal player!
        if (healing > 0)
        {
            PlayerHealth playerHealth =
                GameObject.FindGameObjectWithTag("Player")
                    ?.GetComponent<PlayerHealth>();

            playerHealth?.Heal(healing);
        }

        GetComponent<EnemyAI>().enabled = false;          
        GetComponent<Collider2D>().enabled = false;     

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        GetComponent<EnemyAnimator>().TriggerDeath();     
    }

    public void FinishDying()
    {
        Destroy(gameObject);
    }
    
}
