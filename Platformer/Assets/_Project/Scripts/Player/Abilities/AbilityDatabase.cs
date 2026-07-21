using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Registry of every AbilityNodeData asset. Lives at Assets/Resources/AbilityDatabase.asset
    /// so it can be loaded on first access (same pattern as ItemDatabase).
    /// Use "Auto-populate from Project" (context menu) after adding new nodes.
    /// </summary>
    [CreateAssetMenu(menuName = "Abilities/Ability Database", fileName = "AbilityDatabase")]
    public class AbilityDatabase : ScriptableObject
    {
        static AbilityDatabase _instance;
        public static AbilityDatabase instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<AbilityDatabase>("AbilityDatabase");
                return _instance;
            }
        }

        [SerializeField] List<AbilityNodeData> nodes = new();

        public IReadOnlyList<AbilityNodeData> Nodes => nodes;

        Dictionary<string, AbilityNodeData> lookup;

        void OnEnable()
        {
            _instance = this;
            BuildLookup();
        }

        void BuildLookup()
        {
            lookup = new Dictionary<string, AbilityNodeData>();
            foreach (var node in nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.id)) continue;
                lookup[node.id] = node;
            }
        }

        public AbilityNodeData Find(string id)
        {
            if (lookup == null) BuildLookup();
            return lookup.TryGetValue(id, out var node) ? node : null;
        }

        public AbilityNodeData FindByBranchTier(AbilityBranch branch, int tier)
        {
            foreach (var node in nodes)
                if (node != null && node.branch == branch && node.tier == tier)
                    return node;
            return null;
        }

#if UNITY_EDITOR
        [ContextMenu("Auto-populate from Project")]
        void AutoPopulate()
        {
            nodes.Clear();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:AbilityNodeData");
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<AbilityNodeData>(path);
                if (asset != null) nodes.Add(asset);
            }
            UnityEditor.EditorUtility.SetDirty(this);
            BuildLookup();
            Debug.Log($"AbilityDatabase populated with {nodes.Count} node(s).");
        }
#endif
    }
}
