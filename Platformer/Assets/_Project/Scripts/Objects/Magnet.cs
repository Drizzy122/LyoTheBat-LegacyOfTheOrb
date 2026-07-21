using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class Magnet : MonoBehaviour
    {
        // We will hide this from inspector so it doesn't clutter the view, 
        // but keep it public if other scripts need it.
        [HideInInspector] 
        public List<GameObject> collectables = new List<GameObject>();

        [Header("Settings")]
        public float magnetRadius = 5f; // Renamed from magnetForce to avoid confusion
        public float pullSpeed = 10f;   // How fast they fly to you

        void Start()
        {
            // Find all objects with the tag "Collectible" at the start
            foreach (var collectable in GameObject.FindGameObjectsWithTag("Collectible"))
            {
                collectables.Add(collectable);
            }
        }

        /// <summary>Register a runtime-spawned collectable (e.g. XP orbs dropped by
        /// enemies) so the magnet can pull it. Safe against duplicates.</summary>
        public void Register(GameObject collectable)
        {
            if (collectable != null && !collectables.Contains(collectable))
                collectables.Add(collectable);
        }

        void Update()
        {
            // We use a for-loop backwards so we can remove items 
            // from the list safely if they have been destroyed.
            for (int i = collectables.Count - 1; i >= 0; i--)
            {
                // 1. Check if the item still exists (wasn't collected yet)
                if (collectables[i] == null)
                {
                    collectables.RemoveAt(i);
                    continue;
                }

                GameObject coin = collectables[i];
                float distance = Vector3.Distance(transform.position, coin.transform.position);

                // 2. Check if inside the Magnet Radius
                if (distance < magnetRadius)
                {
                    // 3. PULL towards player
                    // MoveTowards creates a constant speed (snappy feel), unlike Lerp.
                    coin.transform.position = Vector3.MoveTowards(
                        coin.transform.position, 
                        transform.position, 
                        pullSpeed * Time.deltaTime
                    );
                }
            }
        }
        
        void OnDrawGizmos()
        {
            Gizmos.color = Color.darkBlue;
            // Use the correct variable name here
            Gizmos.DrawWireSphere(transform.position, magnetRadius);
        }
    }
}