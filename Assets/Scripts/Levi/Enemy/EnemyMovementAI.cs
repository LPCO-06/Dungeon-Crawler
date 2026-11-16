using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(KnockbackReceiver))]
public class EnemyMovementAI : MonoBehaviour
{
    public enum EnemyRole
    {
        LightMelee,
        HeavyMelee,
        Ranged
    }

    [Header("Role & Target")]
    [SerializeField] private EnemyRole role = EnemyRole.LightMelee;
    [SerializeField] private Transform target;

    [Header("Knockback Settings")]
    [Tooltip("Toggle knockback immunity for this enemy (e.g., Heavy Melee brutes or bosses).")]
    [SerializeField] private bool knockbackImmune = false;

    [Header("Detection & Engagement")]
    [Tooltip("Maximum distance at which the enemy notices and tracks the player.")]
    [SerializeField] private float aggroRange = 15f;
    [Tooltip("How fast the enemy rotates to face the player.")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Role Spacing Settings")]
    [Tooltip("Heavy Melee: Closes in until this distance.")]
    [SerializeField] private float heavyStopDistance = 1.5f;

    [Tooltip("Light Melee: Preferred combat range (min = retreat threshold, max = advance threshold).")]
    [SerializeField] private Vector2 lightMeleeRange = new Vector2(2.2f, 3.5f);

    [Tooltip("Ranged: Preferred firing zone (min = kite backward threshold, max = advance threshold).")]
    [SerializeField] private Vector2 rangedCombatRange = new Vector2(7.0f, 10.5f);

    [Header("Movement Speeds")]
    [SerializeField] private float advanceSpeed = 4.5f;
    [SerializeField] private float retreatSpeed = 3.2f;

    private NavMeshAgent agent;
    private KnockbackReceiver knockbackReceiver;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();

        agent.updateRotation = false;
        knockbackReceiver.IsKnockbackImmune = knockbackImmune;
    }

    private void OnValidate()
    {
        // Keeps KnockbackReceiver synced when toggling the checkbox in the Inspector
        if (TryGetComponent<KnockbackReceiver>(out KnockbackReceiver receiver))
        {
            receiver.IsKnockbackImmune = knockbackImmune;
        }
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
    }

    private void Update()
    {
        // Keep immunity synced if changed at runtime
        knockbackReceiver.IsKnockbackImmune = knockbackImmune;

        // Pause AI steering while actively sliding from a knockback hit
        if (knockbackReceiver.IsKnockedBack)
        {
            if (agent.hasPath) agent.ResetPath();
            return;
        }

        if (target == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (distanceToPlayer > aggroRange)
        {
            agent.ResetPath();
            return;
        }

        FaceTarget();

        switch (role)
        {
            case EnemyRole.HeavyMelee:
                HandleHeavyMelee(distanceToPlayer);
                break;

            case EnemyRole.LightMelee:
                HandleSpacingMovement(distanceToPlayer, lightMeleeRange.x, lightMeleeRange.y);
                break;

            case EnemyRole.Ranged:
                HandleSpacingMovement(distanceToPlayer, rangedCombatRange.x, rangedCombatRange.y);
                break;
        }
    }

    private void HandleHeavyMelee(float distance)
    {
        if (distance > heavyStopDistance)
        {
            agent.speed = advanceSpeed;
            agent.SetDestination(target.position);
        }
        else
        {
            agent.ResetPath();
        }
    }

    private void HandleSpacingMovement(float distance, float minRange, float maxRange)
    {
        if (distance > maxRange)
        {
            agent.speed = advanceSpeed;
            agent.SetDestination(target.position);
        }
        else if (distance < minRange)
        {
            agent.speed = retreatSpeed;
            Vector3 dirAwayFromPlayer = (transform.position - target.position).normalized;
            Vector3 retreatPoint = transform.position + dirAwayFromPlayer * (minRange - distance + 1f);

            if (NavMesh.SamplePosition(retreatPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            agent.ResetPath();
        }
    }

    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        if (role == EnemyRole.HeavyMelee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, heavyStopDistance);
        }
        else
        {
            Vector2 range = (role == EnemyRole.LightMelee) ? lightMeleeRange : rangedCombatRange;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range.x);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, range.y);
        }
    }
}