using UnityEngine;
using UnityEngine.AI;

public class KnockbackReceiver : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("If true, this entity ignores all incoming knockback forces.")]
    [SerializeField] private bool isKnockbackImmune = false;

    [Tooltip("How quickly the knockback slide slows down to a stop.")]
    [SerializeField] private float drag = 12f;

    private NavMeshAgent agent;
    private CharacterController characterController;
    private Vector3 impactVelocity;

    public bool IsKnockbackImmune
    {
        get => isKnockbackImmune;
        set => isKnockbackImmune = value;
    }

    /// <summary>
    /// True while the entity is actively being pushed back.
    /// </summary>
    public bool IsKnockedBack => impactVelocity.sqrMagnitude > 0.05f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Applies a horizontal knockback impulse in the given direction.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (isKnockbackImmune || force <= 0f) return;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        impactVelocity = direction.normalized * force;
    }

    private void Update()
    {
        if (!IsKnockedBack)
        {
            impactVelocity = Vector3.zero;
            return;
        }

        Vector3 displacement = impactVelocity * Time.deltaTime;

        // Move using whichever movement component is attached
        if (agent != null && agent.enabled)
        {
            // NavMeshAgent.Move automatically slides along walls without leaving the NavMesh
            agent.Move(displacement);
        }
        else if (characterController != null && characterController.enabled)
        {
            characterController.Move(displacement);
        }
        else
        {
            transform.position += displacement;
        }

        // Smoothly damp the knockback velocity back to zero
        impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, drag * Time.deltaTime);
    }
}