using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyMovementAI))]
public class EnemyCombat : MonoBehaviour
{
    [Header("Attack Configuration")]
    [Tooltip("Assign any AbilitySO (MeleeArc, AreaAroundSelf, or Projectile).")]
    [SerializeField] private AbilitySO enemyAttack;
    [Tooltip("Select the Layer(s) assigned to the Player.")]
    [SerializeField] private LayerMask targetLayers;
    [Tooltip("Maximum distance at which the enemy will initiate an attack.")]
    [SerializeField] private float attackRange = 2.5f;

    [Header("Timing & Telegraph")]
    [Tooltip("Brief pause before the attack lands so the player can react.")]
    [SerializeField] private float attackWindup = 0.35f;
    [Tooltip("Time between attacks (overrides or adds to AbilitySO cooldown).")]
    [SerializeField] private float attackInterval = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool showAttackGizmo = true;

    private Transform target;
    private KnockbackReceiver knockbackReceiver;
    private float cooldownTimer;
    private bool isAttacking;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        knockbackReceiver = GetComponent<KnockbackReceiver>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (target == null || enemyAttack == null || isAttacking || cooldownTimer > 0f)
        {
            return;
        }

        // Don't start an attack while being knocked back
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

        // Wind-up delay (hook up an animation trigger or color flash here later!)
        yield return new WaitForSeconds(attackWindup);

        // Cancel if knocked back mid-windup (unless immune)
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

        // Draw engagement trigger range
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw the actual AbilitySO hitbox/trajectory
        if (enemyAttack != null)
        {
            enemyAttack.DrawGizmo(transform, Color.red);
        }
    }
}