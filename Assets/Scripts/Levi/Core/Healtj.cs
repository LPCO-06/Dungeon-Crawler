using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onDeath;

    [Header("Debug")]
    [SerializeField] private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Sets a new max health (e.g., from a Player Class) and resets current health.
    /// </summary>
    public void InitializeHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        isDead = false;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public bool TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return false;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }

        return true;
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        onDeath?.Invoke();
    }
}