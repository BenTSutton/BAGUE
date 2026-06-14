using UnityEngine;

public class RatLogic : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private Transform playerTransform;
    [SerializeField] private float stopDistance = 2f;

    private Vector2 moveDirection;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        if (moveDirection.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void FlipObject()
    {
        if(moveDirection == Vector2.left)
        {
            moveDirection = Vector2.right;
        }
        else
        {
            moveDirection = Vector2.left;
        }
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        Debug.Log(distanceToPlayer);

        bool isRunning = distanceToPlayer > stopDistance;

        animator.SetBool("isRunning", isRunning);

        if (isRunning)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}