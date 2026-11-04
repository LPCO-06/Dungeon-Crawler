using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon Profile Data")]
    [SerializeField] private WeaponData currentWeapon;

    [Header("Transform References")]
    [SerializeField] private Transform swordTransform;
    [SerializeField] private Transform shieldTransform;

    [Header("VFX GameObjects")]
    [SerializeField] private GameObject swordSlashVfx;
    [SerializeField] private GameObject shieldBashVfx;

    private PlayerController playerController;
    private float m1Timer = 0f;
    private float m2Timer = 0f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (m1Timer > 0f) m1Timer -= Time.deltaTime;
        if (m2Timer > 0f) m2Timer -= Time.deltaTime;

        if (Input.GetMouseButton(0) && m1Timer <= 0f)
        {
            ExecuteLightAttack();
        }
        else if (Input.GetMouseButton(1) && m2Timer <= 0f)
        {
            ExecuteHeavyAttack();
        }
    }

    private void ExecuteLightAttack()
    {
        if (currentWeapon == null) return;

        m1Timer = currentWeapon.m1Cooldown;

        // Force reset and play all particle systems on the sword slash object
        PlayVfxObject(swordSlashVfx);

        StartCoroutine(PerformMovingAttack(
            currentWeapon.m1ForwardLunge,
            currentWeapon.m1HitboxOffset,
            currentWeapon.m1HitboxSize,
            currentWeapon.m1Damage,
            currentWeapon.m1KnockbackForce
        ));
    }

    private void ExecuteHeavyAttack()
    {
        if (currentWeapon == null) return;

        m2Timer = currentWeapon.m2Cooldown;

        // Force reset and play all particle systems on the shield bash object
        PlayVfxObject(shieldBashVfx);

        StartCoroutine(PerformMovingAttack(
            currentWeapon.m2ForwardLunge,
            currentWeapon.m2HitboxOffset,
            currentWeapon.m2HitboxSize,
            currentWeapon.m2Damage,
            currentWeapon.m2KnockbackForce
        ));
    }

    private void PlayVfxObject(GameObject vfxObj)
    {
        if (vfxObj == null) return;

        // Ensure the GameObject is active
        vfxObj.SetActive(true);

        // Check for a Visual Effect Graph component
        VisualEffect vfxGraph = vfxObj.GetComponentInChildren<VisualEffect>(true);
        if (vfxGraph != null)
        {
            vfxGraph.Reinit();
            vfxGraph.SendEvent("OnPlay"); // Standard default event name for VFX graphs
            return;
        }

        // Fallback: regular particle systems just in case
        ParticleSystem[] particleSystems = vfxObj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.gameObject.SetActive(true);
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Simulate(0f, true, true);
            ps.Play(true);
        }
    }
    private IEnumerator PerformMovingAttack(float lungeForce, Vector3 offset, Vector3 size, float damage, float knockback)
    {
        float attackDuration = 0.1f;
        float elapsed = 0f;

        while (elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            playerController.ApplyAttackLunge(transform.forward * (lungeForce * Time.deltaTime * 10f));

            Vector3 boxCenter = transform.position + transform.rotation * offset;
            Collider[] hitColliders = Physics.OverlapBox(boxCenter, size / 2f, transform.rotation);

            bool hitAny = false;
            foreach (var hit in hitColliders)
            {
                if (hit.gameObject == gameObject) continue;

                if (hit.TryGetComponent<EnemyHealth>(out var enemy))
                {
                    enemy.TakeDamage(damage, transform.forward, knockback);
                    hitAny = true;
                }
            }

            if (hitAny)
            {
                StartCoroutine(TriggerHitStop(0.08f));
                break;
            }

            yield return null;
        }
    }

    private IEnumerator TriggerHitStop(float duration)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    private void OnDrawGizmosSelected()
    {
        if (currentWeapon == null) return;

        Gizmos.color = Color.yellow;
        Vector3 m1Center = transform.position + transform.rotation * currentWeapon.m1HitboxOffset;
        Gizmos.matrix = Matrix4x4.TRS(m1Center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, currentWeapon.m1HitboxSize);

        Gizmos.color = Color.cyan;
        Vector3 m2Center = transform.position + transform.rotation * currentWeapon.m2HitboxOffset;
        Gizmos.matrix = Matrix4x4.TRS(m2Center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, currentWeapon.m2HitboxSize);
    }
}