using UnityEngine;

[RequireComponent(typeof(Health), typeof(PlayerController), typeof(PlayerCombat))]
public class PlayerClassManager : MonoBehaviour
{
    [Header("Active Class")]
    [SerializeField] private PlayerClassSO startingClass;

    private Health health;
    private PlayerController controller;
    private PlayerCombat combat;

    public PlayerClassSO StartingClass => startingClass;
    public PlayerClassSO CurrentClass { get; private set; }

    private void Awake()
    {
        health = GetComponent<Health>();
        controller = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();
    }

    private void Start()
    {
        if (startingClass != null)
        {
            ApplyClass(startingClass);
        }
    }

    public void ApplyClass(PlayerClassSO newClass)
    {
        if (newClass == null) return;

        CurrentClass = newClass;
        health.InitializeHealth(newClass.maxHealth);
        controller.SetMoveSpeed(newClass.moveSpeed);
        combat.EquipClassAbilities(newClass);

        Debug.Log($"Equipped Class: {newClass.className} (HP: {newClass.maxHealth}, Speed: {newClass.moveSpeed})");
    }
}