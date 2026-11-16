using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerClass", menuName = "DungeonCrawler/Player Class")]
public class PlayerClassSO : ScriptableObject
{
    [Header("Class Identity")]
    public string className = "Wolf";

    [Header("Base Stats")]
    public float maxHealth = 16f;
    public float moveSpeed = 5.5f;

    [Header("4-Slot Ability Kit")]
    public AbilitySO primaryAttack;
    public AbilitySO secondaryAttack;
    public AbilitySO skill1;
    public AbilitySO skill2;
}