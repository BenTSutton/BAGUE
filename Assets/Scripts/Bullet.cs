//Bullet script
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
    public float knockbackForce = 15f; // 10x the melee knockback
    public float knockbackPauseTime = 0.3f; // 10x the melee knockback
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
        Debug.Log("Bullet entered trigger");
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
            return;

        enemyHealth.TakeDamage(damage, EnemyHitType.Gun);

        Rigidbody2D enemyRb = enemyHealth.GetComponent<Rigidbody2D>();

        if (enemyRb != null)
        {
            Debug.Log("Should damage enemy with bullet");
            enemyRb.linearVelocity = Vector2.zero;
            enemyRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

            EnemyAI enemyAI = enemyHealth.GetComponent<EnemyAI>();
            enemyAI.StartCoroutine(enemyAI.KnockbackPause(knockbackPauseTime));
        }

        Destroy(gameObject);
    }
}