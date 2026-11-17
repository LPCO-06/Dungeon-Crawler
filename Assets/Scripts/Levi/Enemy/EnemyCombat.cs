using System.Collections;
using PurrNet;
using UnityEngine;

[RequireComponent(typeof(EnemyMovementAI))]
public class EnemyCombat : NetworkBehaviour
{
    [Header("Attack Configuration")]
    [SerializeField] private AbilitySO enemyAttack;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private float attackRange = 2.5f;

    [Header("Timing & Telegraph")]
    [SerializeField] private float attackWindup = 0.35f;
    [SerializeField] private float attackInterval = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool showAttackGizmo = true;

    private EnemyMovementAI movementAI;
    private KnockbackReceiver knockbackReceiver;
    private float cooldownTimer;
    private bool isAttacking;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        movementAI = GetComponent<EnemyMovementAI>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();
    }

    private void Update()
    {
        if (!isServer) return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        Transform target = movementAI.CurrentTarget;
        if (target == null || enemyAttack == null || isAttacking || cooldownTimer > 0f)
        {
            return;
        }

        if (knockbackReceiver != null && knockbackReceiver.IsKnockedBack)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (distanceToPlayer <= attackRange)
        {
            StartCoroutine(PerformAttackRoutine());
        }
    }

    private IEnumerator PerformAttackRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackWindup);

        if (knockbackReceiver == null || !knockbackReceiver.IsKnockedBack)
        {
            enemyAttack.Activate(transform, targetLayers);
        }

        cooldownTimer = Mathf.Max(attackInterval, enemyAttack.cooldown);
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAttackGizmo) return;

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (enemyAttack != null)
        {
            enemyAttack.DrawGizmo(transform, Color.red);
        }
    }
}