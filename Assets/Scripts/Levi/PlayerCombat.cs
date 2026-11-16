using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Target Detection")]
    [Tooltip("Select the Layer(s) assigned to your Enemy prefabs.")]
    [SerializeField] private LayerMask enemyLayers;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference primaryAction;
    [SerializeField] private InputActionReference secondaryAction;
    [SerializeField] private InputActionReference skill1Action;
    [SerializeField] private InputActionReference skill2Action;

    [Header("Debug Visualization")]
    [SerializeField] private bool showAbilityGizmos = true;

    private AbilitySO[] abilities = new AbilitySO[4];
    private float[] cooldownTimers = new float[4];

    private void OnEnable()
    {
        EnableAction(primaryAction);
        EnableAction(secondaryAction);
        EnableAction(skill1Action);
        EnableAction(skill2Action);
    }

    private void OnDisable()
    {
        DisableAction(primaryAction);
        DisableAction(secondaryAction);
        DisableAction(skill1Action);
        DisableAction(skill2Action);
    }

    private void EnableAction(InputActionReference actionRef)
    {
        if (actionRef != null) actionRef.action.Enable();
    }

    private void DisableAction(InputActionReference actionRef)
    {
        if (actionRef != null) actionRef.action.Disable();
    }

    public void EquipClassAbilities(PlayerClassSO playerClass)
    {
        abilities[0] = playerClass.primaryAttack;
        abilities[1] = playerClass.secondaryAttack;
        abilities[2] = playerClass.skill1;
        abilities[3] = playerClass.skill2;

        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            cooldownTimers[i] = 0f;
        }
    }

    private void Update()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0f)
            {
                cooldownTimers[i] -= Time.deltaTime;
            }
        }

        if (IsTriggered(primaryAction)) TryUseAbility(0);
        if (IsTriggered(secondaryAction)) TryUseAbility(1);
        if (IsTriggered(skill1Action)) TryUseAbility(2);
        if (IsTriggered(skill2Action)) TryUseAbility(3);
    }

    private bool IsTriggered(InputActionReference actionRef)
    {
        return actionRef != null && actionRef.action != null && actionRef.action.WasPressedThisFrame();
    }

    private void TryUseAbility(int slotIndex)
    {
        AbilitySO ability = abilities[slotIndex];
        if (ability == null || cooldownTimers[slotIndex] > 0f) return;

        ability.Activate(transform, enemyLayers);
        cooldownTimers[slotIndex] = ability.cooldown;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAbilityGizmos) return;

        Color[] slotColors = { Color.cyan, Color.green, Color.yellow, Color.magenta };

        // In Play Mode, draw the currently equipped runtime abilities
        if (Application.isPlaying)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] != null)
                {
                    abilities[i].DrawGizmo(transform, slotColors[i]);
                }
            }
        }
        // In Edit Mode, preview the Starting Class assigned to PlayerClassManager
        else if (TryGetComponent<PlayerClassManager>(out PlayerClassManager classManager) && classManager.StartingClass != null)
        {
            PlayerClassSO previewClass = classManager.StartingClass;
            if (previewClass.primaryAttack != null) previewClass.primaryAttack.DrawGizmo(transform, slotColors[0]);
            if (previewClass.secondaryAttack != null) previewClass.secondaryAttack.DrawGizmo(transform, slotColors[1]);
            if (previewClass.skill1 != null) previewClass.skill1.DrawGizmo(transform, slotColors[2]);
            if (previewClass.skill2 != null) previewClass.skill2.DrawGizmo(transform, slotColors[3]);
        }
    }
}