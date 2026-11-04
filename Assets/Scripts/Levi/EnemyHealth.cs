using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Stats")]
    public string enemyName = "Chained";
    public float maxHealth = 50f;
    public float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{enemyName} took {damageAmount} damage! Current HP: {currentHealth}/{maxHealth}");

        // Simple visual hit feedback (flashes red briefly)
        StartCoroutine(HitFlash());

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator HitFlash()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color originalColor = rend.material.color;
            rend.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            rend.material.color = originalColor;
        }
    }

    private void Die()
    {
        Debug.Log($"{enemyName} has been defeated!");
        Destroy(gameObject);
    }
}