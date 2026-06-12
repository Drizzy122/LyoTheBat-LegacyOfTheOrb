using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Abstract base for every item the player can hold. Each concrete item type
    /// (WeaponData, ArmorData, ConsumableData) extends this.
    /// </summary>
    public abstract class ItemData : ScriptableObject
    {
        [field: SerializeField] public string id { get; private set; }
        [field: SerializeField] public string displayName { get; private set; }
        [field: SerializeField, TextArea(2, 5)] public string description { get; private set; }
        [field: SerializeField] public Sprite icon { get; private set; }
        [field: SerializeField] public Rarity rarity { get; private set; } = Rarity.Common;

        [Header("Stacking")]
        [field: SerializeField, Min(1)] public int maxStack { get; private set; } = 1;

#if UNITY_EDITOR
        void OnValidate()
        {
            // Default id to asset name so future persistence has a stable key without manual setup.
            if (string.IsNullOrEmpty(id)) id = name;
        }
#endif
    }
}
