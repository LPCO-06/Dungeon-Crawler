using PurrNet;
using UnityEngine;

public class SimpleProjectile : NetworkBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private float damage;
    private float speed;
    private float knockbackForce;
    private LayerMask targetLayers;
    private float lifeTimer;
    private bool initialized;

    public void Initialize(float dmg, float spd, float knockback, LayerMask layers)
    {
        damage = dmg;
        speed = spd;
        knockbackForce = knockback;
        targetLayers = layers;
        lifeTimer = lifetime;
        initialized = true;
    }

    private void Update()
    {
        if (!isServer || !initialized) return;

        transform.position += transform.forward * (speed * Time.deltaTime);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || !initialized) return;

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

            Despawn();
        }
        else if (!other.isTrigger && !other.CompareTag("Player"))
        {
            Despawn();
        }
    }
}