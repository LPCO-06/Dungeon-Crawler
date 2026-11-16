using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private float damage;
    private float speed;
    private float knockbackForce;
    private LayerMask targetLayers;

    public void Initialize(float dmg, float spd, float knockback, LayerMask layers)
    {
        damage = dmg;
        speed = spd;
        knockbackForce = knockback;
        targetLayers = layers;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            if (other.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(damage);
            }

            if (knockbackForce > 0f && other.TryGetComponent<KnockbackReceiver>(out KnockbackReceiver receiver))
            {
                receiver.ApplyKnockback(transform.forward, knockbackForce);
            }

            Destroy(gameObject);
        }
        else if (!other.isTrigger && !other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}