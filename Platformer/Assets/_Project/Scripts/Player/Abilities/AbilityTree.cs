using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// The player's ability-tree state: skill points, unlocked nodes, and stat totals.
    /// Lives on the Player. Earns 1 skill point per level gained (via onPlayerLevelChange).
    /// Gameplay systems query GetStat(key) for aggregated bonuses.
    /// </summary>
    public class AbilityTree : MonoBehaviour, IDataPersistence
    {
        // Stat keys — gameplay hooks query these.
        public const string StatMeleeDamage = "melee_damage";
        public const string StatBlastProjectiles = "blast_projectiles";
        public const string StatDefense = "defense";

        [Header("Runtime State (visible for debugging)")]
        [SerializeField, Min(0)] int skillPoints;

        int lastSyncedLevel = 1;
        readonly HashSet<string> unlocked = new();

        public int SkillPoints => skillPoints;

        /// <summary>Fired whenever points or unlocks change — UI listens.</summary>
        public event System.Action OnChanged;

        void OnEnable()
        {
            if (GameEventsManager.instance != null)
                GameEventsManager.instance.playerEvents.onPlayerLevelChange += HandleLevelChange;
        }

        void OnDisable()
        {
            if (GameEventsManager.instance != null)
                GameEventsManager.instance.playerEvents.onPlayerLevelChange -= HandleLevelChange;
        }

        void HandleLevelChange(int newLevel)
        {
            // 1 skill point per level gained. lastSyncedLevel guards against the
            // initial level broadcast and against double-granting after load.
            if (newLevel <= lastSyncedLevel) return;
            skillPoints += newLevel - lastSyncedLevel;
            lastSyncedLevel = newLevel;
            OnChanged?.Invoke();
        }

        public bool IsUnlocked(string id) => unlocked.Contains(id);

        public bool PrerequisiteMet(AbilityNodeData node)
        {
            if (node == null) return false;
            if (node.tier <= 1) return true;
            var prev = AbilityDatabase.instance != null
                ? AbilityDatabase.instance.FindByBranchTier(node.branch, node.tier - 1)
                : null;
            return prev == null || IsUnlocked(prev.id);
        }

        public bool CanUnlock(AbilityNodeData node)
        {
            if (node == null || IsUnlocked(node.id)) return false;
            if (skillPoints < node.cost) return false;
            return PrerequisiteMet(node);
        }

        public bool Unlock(AbilityNodeData node)
        {
            if (!CanUnlock(node)) return false;
            skillPoints -= node.cost;
            unlocked.Add(node.id);
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Sum of statValue across all unlocked nodes with this key.</summary>
        public float GetStat(string key)
        {
            var db = AbilityDatabase.instance;
            if (db == null) return 0f;

            float total = 0f;
            foreach (var id in unlocked)
            {
                var node = db.Find(id);
                if (node != null && node.statKey == key)
                    total += node.statValue;
            }
            return total;
        }

        // ─── IDataPersistence ──────────────────────────────────────────────────

        public void SaveData(GameData data)
        {
            data.abilitySkillPoints = skillPoints;
            data.abilityLastSyncedLevel = lastSyncedLevel;
            data.abilitiesUnlocked = new List<string>(unlocked);
        }

        public void LoadData(GameData data)
        {
            skillPoints = data.abilitySkillPoints;
            lastSyncedLevel = Mathf.Max(1, data.abilityLastSyncedLevel);
            unlocked.Clear();
            if (data.abilitiesUnlocked != null)
                foreach (var id in data.abilitiesUnlocked)
                    unlocked.Add(id);
            OnChanged?.Invoke();
        }
    }
}
