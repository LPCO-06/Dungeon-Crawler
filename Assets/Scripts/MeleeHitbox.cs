using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public Vector3 attackDirection;

    private float lifetime = 0.3f;
    private float timer;

    private static bool swingAlternator = false;
    private bool currentSwingDir;

    void Start()
    {
        Destroy(gameObject, lifetime);
        currentSwingDir = swingAlternator;
        swingAlternator = !swingAlternator;
    }

    void Update()
    {
        if (playerTransform == null) return;

        timer += Time.deltaTime;
        float progress = timer / lifetime;

        float startAngle = currentSwingDir ? 60f : -60f;
        float endAngle = currentSwingDir ? -60f : 60f;
        float currentAngle = Mathf.Lerp(startAngle, endAngle, progress);

        Quaternion baseRotation = Quaternion.LookRotation(attackDirection);
        Quaternion sweepRotation = baseRotation * Quaternion.Euler(0f, currentAngle, 0f);

        float arcForward = Mathf.Sin(progress * Mathf.PI) * 0.3f;
        float baseDistance = 0.8f;

        Vector3 offsetDir = sweepRotation * Vector3.forward;
        transform.position = playerTransform.position + offsetDir * (baseDistance + arcForward) + Vector3.up * 0.5f;
        transform.rotation = sweepRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}