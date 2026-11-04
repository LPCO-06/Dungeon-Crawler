using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Active Class")]
    public CharacterClassData characterClass;

    [Header("Movement Smoothing")]
    public float accelerationTime = 0.1f;
    public float rotationSpeed = 720f; // Degrees per second

    [Header("Gravity")]
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;
    private Vector3 moveVelocitySmoothing;

    // Runtime stats derived from ScriptableObject
    private float currentMoveSpeed;
    private float currentHealth;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        ApplyClassData();
    }

    public void ApplyClassData()
    {
        if (characterClass != null)
        {
            currentMoveSpeed = characterClass.moveSpeed;
            currentHealth = characterClass.maxHealth;
            Debug.Log($"Loaded Class: {characterClass.className} | Speed: {currentMoveSpeed} | Attack Power: {characterClass.attackPower}");
        }
        else
        {
            Debug.LogWarning("No Character Class assigned to Player!");
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleCombatInput();
    }

    private void HandleMovement()
    {
        // 1. Dual-support input: Left Stick on Gamepad OR WASD / Arrows
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 targetInput = new Vector3(moveX, 0f, moveZ);

        if (targetInput.magnitude > 1f)
        {
            targetInput.Normalize();
        }

        // 2. Smooth acceleration / deceleration
        Vector3 targetVelocity = targetInput * currentMoveSpeed;
        currentMoveVelocity = Vector3.SmoothDamp(
            currentMoveVelocity,
            targetVelocity,
            ref moveVelocitySmoothing,
            accelerationTime
        );

        // 3. Move controller
        controller.Move(currentMoveVelocity * Time.deltaTime);

        // 4. Controller-friendly rotation facing movement vector
        if (targetInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetInput);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // 5. Gravity handling
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCombatInput()
    {
        // Primary Attack: Left Mouse Button OR Gamepad Right Trigger / West Button ("Fire1")
        if (Input.GetButtonDown("Fire1"))
        {
            PerformPrimaryAttack();
        }

        // Secondary Attack: Right Mouse Button OR Gamepad Left Trigger / North Button ("Fire2")
        if (Input.GetButtonDown("Fire2"))
        {
            PerformSecondaryAttack();
        }
    }

    [Header("Combat Settings")]
    public float attackRange = 2f;
    public LayerMask enemyLayer;

    private void PerformPrimaryAttack()
    {
        Debug.Log($"[Primary Attack] {characterClass?.primaryAttackName} used!");

        // Detect enemies in front of the player using an OverlapSphere
        Vector3 attackPoint = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint, attackRange * 0.5f, enemyLayer);

        foreach (Collider enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();
            if (enemy != null && characterClass != null)
            {
                enemy.TakeDamage(characterClass.attackPower);
            }
        }
    }

    // Draw attack sphere in Scene view for easy debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackPoint = transform.position + transform.forward * (attackRange * 0.5f);
        Gizmos.DrawWireSphere(attackPoint, attackRange * 0.5f);
    }

    private void PerformSecondaryAttack()
    {
        Debug.Log($"[Secondary Attack] {characterClass?.secondaryAttackName} activated!");
    }

    public void SwitchClass(CharacterClassData newClass)
    {
        characterClass = newClass;
        ApplyClassData();
    }
}