using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    [System.Serializable]
    public class GameData
    {
        public long lastUpdated;
        public Vector3 playerPosition;
        // collectables 
        public SerializableDictionary<string, bool> coinsCollected;
        public SerializableDictionary<string, bool> ecliptiumCollected;
        public SerializableDictionary<string, bool> luminCollected;


        // the values defined in this constructor will be the default values
        // the game starts with when there's no data to load
        public GameData()
        {
            playerPosition = Vector3.zero;
            //for collectables
            coinsCollected = new SerializableDictionary<string, bool>();
            ecliptiumCollected = new SerializableDictionary<string, bool>();
            luminCollected = new SerializableDictionary<string, bool>();
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