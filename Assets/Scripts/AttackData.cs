using UnityEngine;

public enum AttackType { Melee, Ranged }

[CreateAssetMenu(fileName = "NewAttack", menuName = "ScriptableObjects/AttackData")]
public class AttackData : ScriptableObject
{
    [Header("Algemeen")]
    public string attackName;
    public AttackType attackType;
    public GameObject attackPrefab;

    [Header("Aanval Stats")]
    public float damageMultiplier = 1f;
    public float cooldown = 0.5f;
    public float range = 1f;
}