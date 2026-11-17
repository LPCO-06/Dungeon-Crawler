using System.Collections.Generic;
using PurrNet;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(PlayerController), typeof(PlayerCombat))]
public class PlayerClassManager : NetworkBehaviour
{
    [Header("Available Classes")]
    [Tooltip("Assign all playable PlayerClassSO assets here so class indices sync across the network.")]
    [SerializeField] private List<PlayerClassSO> availableClasses = new List<PlayerClassSO>();
    [SerializeField] private int defaultClassIndex = 0;

    private SyncVar<int> activeClassIndex = new SyncVar<int>(0);

    private Health health;
    private PlayerController controller;
    private PlayerCombat combat;

    public PlayerClassSO StartingClass => (availableClasses.Count > defaultClassIndex && defaultClassIndex >= 0)
        ? availableClasses[defaultClassIndex]
        : null;

    public PlayerClassSO CurrentClass { get; private set; }

    private void Awake()
    {
        health = GetComponent<Health>();
        controller = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        activeClassIndex.onChanged += OnClassIndexChanged;

        if (isServer)
        {
            activeClassIndex.value = defaultClassIndex;
        }

        ApplyClassByIndex(activeClassIndex.value);
    }

    protected override void OnDespawned()
    {
        activeClassIndex.onChanged -= OnClassIndexChanged;
        base.OnDespawned();
    }

    private void OnClassIndexChanged(int newIndex)
    {
        ApplyClassByIndex(newIndex);
    }

    /// <summary>
    /// Call this from your Class Selection UI on the local player.
    /// </summary>
    public void RequestClassChange(int classIndex)
    {
        if (!isOwner) return;
        SetClassServerRpc(classIndex);
    }

    [ServerRpc]
    private void SetClassServerRpc(int classIndex)
    {
        if (classIndex >= 0 && classIndex < availableClasses.Count)
        {
            activeClassIndex.value = classIndex;
        }
    }

    private void ApplyClassByIndex(int index)
    {
        if (index < 0 || index >= availableClasses.Count) return;

        PlayerClassSO chosenClass = availableClasses[index];
        if (chosenClass == null) return;

        CurrentClass = chosenClass;
        health.InitializeHealth(chosenClass.maxHealth);
        controller.SetMoveSpeed(chosenClass.moveSpeed);
        combat.EquipClassAbilities(chosenClass);
    }
}