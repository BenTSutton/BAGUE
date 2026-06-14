//Bullet script
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
    public float knockbackForce = 15f; // 10x the melee knockback
    public float lifetime = 3f;

    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime); // clean up if it misses
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);

            // Knockback the enemy
            Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = Vector2.zero;
                enemyRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                other.GetComponent<EnemyAI>()?.StartCoroutine("KnockbackPause");
            }

            Destroy(gameObject);
        }
    }
}