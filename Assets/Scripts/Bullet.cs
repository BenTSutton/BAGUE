//Bullet script
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 25f;
    public int damage = 1;
    public float knockbackForce = 15f;
    public float lifetime = 3f;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // stops tunneling
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        rb.linearVelocity = direction * speed; // move via physics, not Translate
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth == null) return;

        enemyHealth.TakeDamage(damage);

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