using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    private bool isDead = false;

    public void TakeDamage(int damage)
    {
        if (isDead) return; //HP CHECK

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die() //What happens when die. 
    {
        isDead = true;
        GetComponent<EnemyAI>().enabled = false;          
        GetComponent<Collider2D>().enabled = false;     

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        GetComponent<EnemyAnimator>().TriggerDeath();     
        StartCoroutine(DestroyAfterAnimation());
    }

    IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1f); //Change this to fit greens ani clip
        Destroy(gameObject);
    }
}