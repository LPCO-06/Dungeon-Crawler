using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Identificatie")]
    public string enemyID = "John_01";

    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("AI Movement")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 1.2f; // Stopt dichtbij de speler
    private Transform playerTransform;

    void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Geen object met de tag 'Player' gevonden in de scène!");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Bereken de afstand tot de speler
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > stoppingDistance)
        {
            // Richting naar de speler bepalen
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0f; // Houd de beweging plat op de grond

            // Beweeg richting de speler
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Draai soepel mee in de looprichting
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{enemyID} geraakt ! {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{enemyID} is verslagen!");
        Destroy(gameObject);
    }
}