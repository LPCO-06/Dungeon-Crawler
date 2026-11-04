using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponProfile", menuName = "ToRiseToFall/Weapon Profile")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "Sword & Shield";

    [Header("M1 Light Attack (Sword Slash)")]
    public float m1Damage = 12f;
    public float m1Cooldown = 0.35f;
    public float m1ForwardLunge = 6f;
    public float m1KnockbackForce = 14f;
    public Vector3 m1HitboxSize = new Vector3(2f, 1.2f, 2.2f);
    public Vector3 m1HitboxOffset = new Vector3(0f, 0f, 1.2f);

    [Header("M2 Heavy Attack (Shield Bash)")]
    public float m2Damage = 28f;
    public float m2Cooldown = 0.9f;
    public float m2ForwardLunge = 12f;
    public float m2KnockbackForce = 25f;
    public Vector3 m2HitboxSize = new Vector3(2.5f, 1.5f, 2.5f);
    public Vector3 m2HitboxOffset = new Vector3(0f, 0f, 1.5f);
}