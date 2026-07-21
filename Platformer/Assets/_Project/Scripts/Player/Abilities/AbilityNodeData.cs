using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// One node in the ability tree. Nodes in the same branch unlock in tier order
    /// (tier 1 first). The effect is expressed as a stat key + value that
    /// AbilityTree.GetStat() aggregates — gameplay systems query the total.
    /// Stat keys in use: AbilityTree.StatMeleeDamage / StatBlastProjectiles / StatDefense.
    /// </summary>
    [CreateAssetMenu(menuName = "Abilities/Ability Node", fileName = "NewAbilityNode")]
    public class AbilityNodeData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public Sprite icon;

        [Header("Tree placement")]
        public AbilityBranch branch = AbilityBranch.Combat;
        [Min(1)] public int tier = 1;

        [Header("Unlock")]
        [Min(1)] public int cost = 1;

        [Header("Effect")]
        public string statKey;
        public float statValue;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (string.IsNullOrEmpty(id)) id = name;
        }
#endif
    }
}
