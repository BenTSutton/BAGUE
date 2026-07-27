using System;
using UnityEngine;

public class RoomHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public int currentHealth;
    public bool IsDestroyed => currentHealth <= 0;

    public event Action<int, int> HealthChanged;
    public event Action Destroyed;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (IsDestroyed || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (IsDestroyed)
            Destroyed?.Invoke();
    }
}