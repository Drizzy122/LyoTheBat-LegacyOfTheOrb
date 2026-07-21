using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    [System.Serializable]
    public struct InventoryEntry
    {
        public string itemId;
        public int quantity;
    }

    [System.Serializable]
    public class GameData
    {
        public long lastUpdated;
        public Vector3 playerPosition;
        // collectables
        public SerializableDictionary<string, bool> coinsCollected;
        public SerializableDictionary<string, bool> ecliptiumCollected;
        public SerializableDictionary<string, bool> luminCollected;
        // sword (legacy — superseded by inventoryItems, kept for back-compat with old saves)
        public bool hasSword;
        // inventory & equipment
        public List<InventoryEntry> inventoryItems;
        public SerializableDictionary<string, string> equippedSlots; // slot.ToString() → itemId
        // generic world pickups (Health / Item collectables) — keyed by collectable id
        public SerializableDictionary<string, bool> collectablesCollected;
        // ability tree
        public int abilitySkillPoints;
        public int abilityLastSyncedLevel;
        public List<string> abilitiesUnlocked;
        // player level & experience
        public int playerLevel;
        public int playerExperience;
        // quests — questId → serialized QuestData (same JsonUtility payload the
        // old PlayerPrefs path used, so the format is unchanged)
        public SerializableDictionary<string, string> questData;
        // notifications
        public SerializableDictionary<string, bool> notificationsTriggered;


        // the values defined in this constructor will be the default values
        // the game starts with when there's no data to load
        public GameData()
        {
            playerPosition = Vector3.zero;
            //for collectables
            coinsCollected = new SerializableDictionary<string, bool>();
            ecliptiumCollected = new SerializableDictionary<string, bool>();
            luminCollected = new SerializableDictionary<string, bool>();
            // sword & notifications
            hasSword = false;
            // inventory & equipment
            inventoryItems = new List<InventoryEntry>();
            equippedSlots = new SerializableDictionary<string, string>();
            collectablesCollected = new SerializableDictionary<string, bool>();
            // ability tree
            abilitySkillPoints = 0;
            abilityLastSyncedLevel = 1;
            abilitiesUnlocked = new List<string>();
            // player level & experience
            playerLevel = 1;
            playerExperience = 0;
            // quests
            questData = new SerializableDictionary<string, string>();
            notificationsTriggered = new SerializableDictionary<string, bool>();
        }

        public int GetPercentageComplete()
        {
            int totalItemsFound = 0;
            int totalItemsPossible = 0;

            // Use a helper to add up the counts from all dictionaries
            CountDictionary(coinsCollected, ref totalItemsFound, ref totalItemsPossible);
            CountDictionary(ecliptiumCollected, ref totalItemsFound, ref totalItemsPossible);
            CountDictionary(luminCollected, ref totalItemsFound, ref totalItemsPossible);

            // Prevent division by zero
            if (totalItemsPossible == 0) return 0;

            // Calculate overall percentage
            return (totalItemsFound * 100 / totalItemsPossible);
        }
        
        private void CountDictionary(SerializableDictionary<string, bool> dict, ref int found, ref int total)
        {
            total += dict.Count;
            foreach (bool isCollected in dict.Values)
            {
                if (isCollected)
                {
                    found++;
                }
            }
        }
    }
}