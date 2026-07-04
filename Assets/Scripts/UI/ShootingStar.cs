using UnityEngine;

public class ShootingStar : MonoBehaviour
{
    [SerializeField] private Vector2 direction = new(1f, -0.4f);
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialise(Vector2 newDirection, float newSpeed)
    {
        direction = newDirection.normalized;
        speed = newSpeed;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}