using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private PlayerController3D playerController;

    private float m1CooldownTimer;
    private float m2CooldownTimer;
    private float skill1CooldownTimer;
    private float skill2CooldownTimer;

    void Start()
    {
        playerController = GetComponent<PlayerController3D>();
    }

    void Update()
    {
        if (playerController == null || playerController.classData == null) return;
        if (Mouse.current == null || Keyboard.current == null) return;

        if (m1CooldownTimer > 0) m1CooldownTimer -= Time.deltaTime;
        if (m2CooldownTimer > 0) m2CooldownTimer -= Time.deltaTime;
        if (skill1CooldownTimer > 0) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0) skill2CooldownTimer -= Time.deltaTime;

        if (Mouse.current.leftButton.isPressed && m1CooldownTimer <= 0)
        {
            if (playerController.classData.primaryAbility != null)
            {
                ExecuteAbility(playerController.classData.primaryAbility, out m1CooldownTimer);
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && m2CooldownTimer <= 0)
        {
            if (playerController.classData.secondaryAbility != null)
            {
                ExecuteAbility(playerController.classData.secondaryAbility, out m2CooldownTimer);
            }
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame && skill1CooldownTimer <= 0)
        {
            if (playerController.classData.skill1Ability != null)
            {
                ExecuteAbility(playerController.classData.skill1Ability, out skill1CooldownTimer);
            }
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame && skill2CooldownTimer <= 0)
        {
            if (playerController.classData.skill2Ability != null)
            {
                ExecuteAbility(playerController.classData.skill2Ability, out skill2CooldownTimer);
            }
        }
    }

    void ExecuteAbility(AbilityData ability, out float cooldownTimer)
    {
        cooldownTimer = ability.cooldown;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 directionToMouse = (mouseWorldPos - transform.position);
        directionToMouse.y = 0f;
        if (directionToMouse == Vector3.zero) directionToMouse = transform.forward;
        directionToMouse.Normalize();

        transform.rotation = Quaternion.LookRotation(directionToMouse);

        float finalDamage = playerController.classData.attackDamage * ability.damageMultiplier;

        // Crit kans wordt nu uitgelezen vanuit het AbilityData script!
        if (Random.Range(0f, 100f) <= ability.critChance)
        {
            finalDamage *= ability.critMultiplier;
            Debug.Log("CRITICAL HIT!");
        }

        if (ability.abilityPrefab != null)
        {
            Vector3 spawnPos = transform.position + directionToMouse * 0.8f + Vector3.up * 0.5f;
            GameObject spawnedObj = Instantiate(ability.abilityPrefab, spawnPos, transform.rotation);

            MeleeHitbox meleeHitbox = spawnedObj.GetComponent<MeleeHitbox>();
            if (meleeHitbox != null)
            {
                meleeHitbox.damage = finalDamage;
                meleeHitbox.playerTransform = transform;
                meleeHitbox.attackDirection = directionToMouse;
            }

            Projectile projectile = spawnedObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.damage = finalDamage;
                projectile.direction = directionToMouse;
                projectile.speed = ability.rangeOrSpeed;
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return transform.position + transform.forward;
    }
}