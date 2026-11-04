using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Death Polish")]
    [SerializeField] private GameObject deathDustVfxPrefab;

    private Rigidbody rb;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, Vector3 hitDirection, float knockbackForce)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Apply knockback via Rigidbody
        if (rb != null)
        {
            // Zero out current velocity so the knockback force hits consistently
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(hitDirection * knockbackForce, ForceMode.Impulse);
        }

        if (currentHealth <= 0f)
        {
            StartCoroutine(DieSequence());
        }
    }

    private IEnumerator DieSequence()
    {
        isDead = true;

        // Optional: briefly pause movement/AI here if you have an enemy movement script

        // Wait a tiny fraction of a second so the knockback physics impulse is actually felt
        yield return new WaitForSeconds(0.08f);

        // Spawn the dust cloud VFX at the enemy's exact position
        if (deathDustVfxPrefab != null)
        {
            GameObject dust = Instantiate(deathDustVfxPrefab, transform.position, Quaternion.identity);
            Destroy(dust, 2f);
        }

        // Destroy the enemy object
        Destroy(gameObject);
    }
}