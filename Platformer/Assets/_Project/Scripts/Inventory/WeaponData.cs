using UnityEngine;

namespace Platformer
{
    [CreateAssetMenu(menuName = "Inventory/Weapon", fileName = "NewWeapon")]
    public class WeaponData : ItemData
    {
        [Header("Weapon — Slot")]
        [Tooltip("Which weapon slot this item equips into. Pick Weapon for a primary, " +
                 "SecondaryWeapon for an off-hand, or Shield.")]
        [field: SerializeField] public EquipSlot slot { get; private set; } = EquipSlot.Weapon;

        [Header("Weapon — Visual")]
        [field: SerializeField] public GameObject weaponPrefab { get; private set; }

        [Header("Weapon — Stats")]
        [field: SerializeField, Min(0)] public int damage { get; private set; } = 10;
        [field: SerializeField, Min(0f)] public float attackSpeed { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float range { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float knockback { get; private set; } = 0.5f;
    }
}
