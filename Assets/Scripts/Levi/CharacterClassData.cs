using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Game/Character Class Data")]
public class CharacterClassData : ScriptableObject
{
    [Header("Class Info")]
    public string className;
    [TextArea] public string classDescription;

    [Header("Base Stats")]
    public float maxHealth = 20f;
    public float moveSpeed = 5f;
    public float attackPower = 10f;
    public float attackCooldown = 0.5f;

    [Header("Abilities & Equipment")]
    public string primaryAttackName;
    public string secondaryAttackName;
}