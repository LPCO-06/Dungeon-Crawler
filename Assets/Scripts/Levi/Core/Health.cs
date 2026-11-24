using PurrNet;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    // SyncVar automatically synchronizes health & death state from Server to all Clients
    private SyncVar<float> currentHealth = new SyncVar<float>(100f);
    private SyncVar<bool> isDead = new SyncVar<bool>(false);

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onDeath;

    public float CurrentHealth => currentHealth.value;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead.value;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        // Subscribe to SyncVar changes so UI/visuals update on every client
        currentHealth.onChanged += HandleHealthChanged;
        isDead.onChanged += HandleDeathChanged;

        if (isServer)
        {
            currentHealth.value = maxHealth;
            isDead.value = false;
        }
    }

    protected override void OnDespawned()
    {
        currentHealth.onChanged -= HandleHealthChanged;
        isDead.onChanged -= HandleDeathChanged;
        base.OnDespawned();
    }

    private void HandleHealthChanged(float newHealth)
    {
        onHealthChanged?.Invoke(newHealth, maxHealth);
    }

    private void HandleDeathChanged(bool dead)
    {
        if (dead) onDeath?.Invoke();
    }

    public void InitializeHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        if (isServer)
        {
            currentHealth.value = maxHealth;
            isDead.value = false;
        }
    }

    /// <summary>
    /// Server-authoritative damage application.
    /// </summary>
    public bool TakeDamage(float amount)
    {
        if (!isServer || isDead.value || amount <= 0f) return false;

        currentHealth.value = Mathf.Clamp(currentHealth.value - amount, 0f, maxHealth);

        if (currentHealth.value <= 0f)
        {
            isDead.value = true;
            Debug.Log($"{gameObject.name} has died.");
        }

        return true;
    }

    public void Heal(float amount)
    {
        if (!isServer || isDead.value || amount <= 0f) return;

        currentHealth.value = Mathf.Clamp(currentHealth.value + amount, 0f, maxHealth);
    }
}