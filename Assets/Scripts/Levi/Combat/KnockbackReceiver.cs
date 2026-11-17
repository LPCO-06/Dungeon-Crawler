using PurrNet;
using UnityEngine;
using UnityEngine.AI;

public class KnockbackReceiver : NetworkBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private bool isKnockbackImmune = false;
    [SerializeField] private float drag = 12f;

    private NavMeshAgent agent;
    private CharacterController characterController;
    private Vector3 impactVelocity;

    public bool IsKnockbackImmune
    {
        get => isKnockbackImmune;
        set => isKnockbackImmune = value;
    }

    public bool IsKnockedBack => impactVelocity.sqrMagnitude > 0.05f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Call this on the Server; broadcasts the knockback impulse so both Server NavMeshAgents
    /// and Owner-Authoritative Players slide smoothly.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (!isServer || isKnockbackImmune || force <= 0f) return;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        ApplyKnockbackRpc(direction.normalized, force);
    }

    [ObserversRpc]
    private void ApplyKnockbackRpc(Vector3 normalizedDir, float force)
    {
        if (isKnockbackImmune) return;
        impactVelocity = normalizedDir * force;
    }

    private void Update()
    {
        if (!IsKnockedBack)
        {
            impactVelocity = Vector3.zero;
            return;
        }

        Vector3 displacement = impactVelocity * Time.deltaTime;

        if (agent != null && agent.enabled && isServer)
        {
            agent.Move(displacement);
        }
        else if (characterController != null && characterController.enabled && isOwner)
        {
            characterController.Move(displacement);
        }

        impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, drag * Time.deltaTime);
    }
}