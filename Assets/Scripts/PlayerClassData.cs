using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerClass", menuName = "ScriptableObjects/PlayerClass")]
public class PlayerClassData : ScriptableObject
{
    [Header("General Info")]
    public string className;
    [TextArea] public string description;

    [Header("Base Stats")]
    public float maxHealth = 20f;
    public float moveSpeed = 10f;

    [Header("Combat Stats")]
    public float attackDamage = 10f;
    public float attackSpeed = 1f;
    [Range(0f, 100f)] public float critChance = 5f;
    public float critMultiplier = 1.5f;
}