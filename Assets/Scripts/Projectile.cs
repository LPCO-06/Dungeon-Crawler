using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public float speed = 15f;

    private float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // Kogel vernietigen bij raak
        }
    }
}