using UnityEngine;

public enum AttackType
{
    MeleeArc,       // Hits enemies in front of the player within radius & angle
    AreaAroundSelf, // Hits all enemies in a 360-degree radius (e.g., Whirlwind/Nova)
    Projectile      // Spawns a projectile prefab moving forward
}

[CreateAssetMenu(fileName = "NewAbility", menuName = "DungeonCrawler/Ability")]
public class AbilitySO : ScriptableObject
{
    [Header("General Info")]
    public string abilityName = "Slash";
    public AttackType attackType = AttackType.MeleeArc;
    public float cooldown = 0.5f;
    public float damage = 20f;
    [Tooltip("Impulse strength applied to targets with a KnockbackReceiver.")]
    public float knockbackForce = 6f;

    [Header("Melee / AoE Settings")]
    public float range = 2.5f;
    [Range(0f, 360f)] public float hitAngle = 120f;
    public Vector3 originOffset = new Vector3(0f, 1f, 0.1f);

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 14f;

    public void Activate(Transform caster, LayerMask enemyLayers)
    {
        if (attackType == AttackType.Projectile)
        {
            FireProjectile(caster, enemyLayers);
        }
        else
        {
            PerformMeleeOrAoE(caster, enemyLayers);
        }
    }

    private void PerformMeleeOrAoE(Transform caster, LayerMask enemyLayers)
    {
        Vector3 hitOrigin = caster.position + caster.TransformDirection(originOffset);
        Collider[] hits = Physics.OverlapSphere(hitOrigin, range, enemyLayers);

        foreach (Collider hit in hits)
        {
            if (hit.transform == caster) continue;

            if (attackType == AttackType.MeleeArc)
            {
                Vector3 dirToTarget = (hit.transform.position - caster.position).normalized;
                dirToTarget.y = 0f;
                float angle = Vector3.Angle(caster.forward, dirToTarget);
                if (angle > hitAngle * 0.5f) continue;
            }

            if (hit.TryGetComponent<Health>(out Health targetHealth))
            {
                targetHealth.TakeDamage(damage);
            }

            // Apply knockback feedback
            if (knockbackForce > 0f && hit.TryGetComponent<KnockbackReceiver>(out KnockbackReceiver receiver))
            {
                Vector3 pushOrigin = (attackType == AttackType.AreaAroundSelf) ? hitOrigin : caster.position;
                Vector3 pushDir = (hit.transform.position - pushOrigin);
                pushDir.y = 0f;

                if (pushDir.sqrMagnitude < 0.001f) pushDir = caster.forward;

                receiver.ApplyKnockback(pushDir.normalized, knockbackForce);
            }
        }
    }

    private void FireProjectile(Transform caster, LayerMask enemyLayers)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"Ability '{abilityName}' is set to Projectile, but no prefab is assigned!");
            return;
        }

        Vector3 spawnPos = caster.position + caster.TransformDirection(originOffset);
        GameObject projObj = Instantiate(projectilePrefab, spawnPos, caster.rotation);

        if (projObj.TryGetComponent<SimpleProjectile>(out SimpleProjectile proj))
        {
            proj.Initialize(damage, projectileSpeed, knockbackForce, enemyLayers);
        }
    }

    public void DrawGizmo(Transform caster, Color gizmoColor)
    {
        Gizmos.color = gizmoColor;
        Vector3 hitOrigin = caster.position + caster.TransformDirection(originOffset);

        if (attackType == AttackType.Projectile)
        {
            Gizmos.DrawWireSphere(hitOrigin, 0.25f);
            Gizmos.DrawRay(hitOrigin, caster.forward * 4f);
        }
        else if (attackType == AttackType.AreaAroundSelf)
        {
            Gizmos.DrawWireSphere(hitOrigin, range);
        }
        else if (attackType == AttackType.MeleeArc)
        {
            Gizmos.DrawWireSphere(hitOrigin, range);

            float halfAngle = hitAngle * 0.5f;
            Vector3 leftRay = Quaternion.Euler(0f, -halfAngle, 0f) * caster.forward;
            Vector3 rightRay = Quaternion.Euler(0f, halfAngle, 0f) * caster.forward;

            Gizmos.DrawRay(hitOrigin, leftRay * range);
            Gizmos.DrawRay(hitOrigin, rightRay * range);
        }
    }
}