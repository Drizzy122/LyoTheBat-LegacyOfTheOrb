using UnityEngine;

namespace Platformer
{
    [CreateAssetMenu(menuName = "Inventory/Armor", fileName = "NewArmor")]
    public class ArmorData : ItemData
    {
        [Header("Armor — Slot")]
        [field: SerializeField] public EquipSlot slot { get; private set; } = EquipSlot.Chest;

        [Header("Armor — Stats")]
        [field: SerializeField, Min(0)] public int defense { get; private set; }
        [field: SerializeField] public int healthBonus { get; private set; }
        [field: SerializeField] public int mobilityBonus { get; private set; }

        // Visual fields are optional for Phase 1 (stats-only).
        // Filled in later when armor visuals are implemented; no rewrite needed.
        [Header("Visual (optional, used later)")]
        [field: SerializeField] public GameObject armorPrefab { get; private set; }
        [field: SerializeField] public Material armorMaterial { get; private set; }
    }
}
