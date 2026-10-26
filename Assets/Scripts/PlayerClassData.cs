using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerClass", menuName = "ScriptableObjects/PlayerClass")]
public class PlayerClassData : ScriptableObject
{
    [Header("General Info")]
    public string className;
    [TextArea] public string description;

    [Header("Base Stats")]
    public float maxHealth = 80f;
    public float currentHealth = 20f;
    public float moveSpeed = 10f;

    [Header("Combat Stats")]
    public float attackDamage = 10f;

    [Header("Default Loadout")]
    public AbilityData primaryAbility;
    public AbilityData secondaryAbility;
    public AbilityData skill1Ability;
    public AbilityData skill2Ability;
}