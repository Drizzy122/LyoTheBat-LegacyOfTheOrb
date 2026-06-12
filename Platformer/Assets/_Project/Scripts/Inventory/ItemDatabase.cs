using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Registry of every ItemData asset in the game, used to resolve string IDs
    /// back to ItemData references at save/load time.
    ///
    /// Place this asset at Assets/Resources/ItemDatabase.asset so Resources.Load
    /// can find it on first access. Use the "Auto-populate from Project" context
    /// menu (right-click the asset header in the inspector) to scan the project
    /// for every ItemData and fill the list automatically.
    /// </summary>
    [CreateAssetMenu(menuName = "Inventory/Item Database", fileName = "ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        static ItemDatabase _instance;
        public static ItemDatabase instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                return _instance;
            }
        }

        [SerializeField] List<ItemData> items = new();

        Dictionary<string, ItemData> lookup;

        void OnEnable()
        {
            _instance = this;
            BuildLookup();
        }

        void BuildLookup()
        {
            lookup = new Dictionary<string, ItemData>();
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrEmpty(item.id)) continue;
                lookup[item.id] = item;
            }
        }

        public ItemData Find(string id)
        {
            if (lookup == null) BuildLookup();
            return lookup.TryGetValue(id, out var data) ? data : null;
        }

#if UNITY_EDITOR
        [ContextMenu("Auto-populate from Project")]
        void AutoPopulate()
        {
            items.Clear();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:ItemData");
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (asset != null) items.Add(asset);
            }
            UnityEditor.EditorUtility.SetDirty(this);
            BuildLookup();
            Debug.Log($"ItemDatabase populated with {items.Count} item(s).");
        }
#endif
    }
}
