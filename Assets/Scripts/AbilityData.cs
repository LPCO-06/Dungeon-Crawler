using UnityEngine;

public enum AbilityType { Melee, Ranged, Utility, Support }

[CreateAssetMenu(fileName = "NewAbility", menuName = "ScriptableObjects/AbilityData")]
public class AbilityData : ScriptableObject
{
    [Header("Ability Info")]
    public string abilityName;
    [TextArea] public string description;
    public AbilityType abilityType;

    [Header("Combat & Timing")]
    public float cooldown = 1f;
    public float damageMultiplier = 1f;
    public float rangeOrSpeed = 15f;

    [Header("Critical Strike Settings")]
    [Range(0f, 100f)] public float critChance = 5f;
    public float critMultiplier = 1.5f;

    [Header("Assets / Prefabs")]
    public GameObject abilityPrefab;
}