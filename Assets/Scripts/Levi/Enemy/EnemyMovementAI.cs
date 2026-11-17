using PurrNet;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(KnockbackReceiver))]
public class EnemyMovementAI : NetworkBehaviour
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
    [SerializeField] private bool knockbackImmune = false;

    [Header("Detection & Engagement")]
    [SerializeField] private float aggroRange = 15f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Role Spacing Settings")]
    [SerializeField] private float heavyStopDistance = 1.5f;
    [SerializeField] private Vector2 lightMeleeRange = new Vector2(2.2f, 3.5f);
    [SerializeField] private Vector2 rangedCombatRange = new Vector2(7.0f, 10.5f);

    [Header("Movement Speeds")]
    [SerializeField] private float advanceSpeed = 4.5f;
    [SerializeField] private float retreatSpeed = 3.2f;

    private NavMeshAgent agent;
    private KnockbackReceiver knockbackReceiver;

    public Transform CurrentTarget => target;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();

        agent.updateRotation = false;
        knockbackReceiver.IsKnockbackImmune = knockbackImmune;
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        // Disable NavMeshAgent on non-host clients so NetworkTransform drives position cleanly
        if (!isServer && agent != null)
        {
            agent.enabled = false;
        }
    }

    private void OnValidate()
    {
        if (TryGetComponent<KnockbackReceiver>(out KnockbackReceiver receiver))
        {
            receiver.IsKnockbackImmune = knockbackImmune;
        }
    }

    private void Update()
    {
        if (!isServer) return;

        knockbackReceiver.IsKnockbackImmune = knockbackImmune;

        if (knockbackReceiver.IsKnockedBack)
        {
            if (agent.hasPath) agent.ResetPath();
            return;
        }

        FindClosestLivingPlayer();
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

    private void FindClosestLivingPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float closestDistSqr = Mathf.Infinity;

        foreach (GameObject playerObj in players)
        {
            if (playerObj.TryGetComponent<Health>(out Health playerHealth) && playerHealth.IsDead)
                continue;

            float distSqr = (playerObj.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = playerObj.transform;
            }
        }

        target = closest;
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