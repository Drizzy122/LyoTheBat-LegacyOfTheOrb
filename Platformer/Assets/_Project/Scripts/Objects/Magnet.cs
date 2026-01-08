using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class Magnet : MonoBehaviour
    {
        public List<GameObject> collectables = new List<GameObject>();
        public float magnetForce;
        public float speed;


        void Start()
        {
            foreach (var collectable in GameObject.FindGameObjectsWithTag("Collectible"))
            {
                collectables.Add(collectable);
            }
        }

        void Update()
        {
            foreach (var collectable in collectables)
            {
                float distance = Vector3.Distance(transform.position, collectable.transform.position);
                if (distance < magnetForce)
                {
                    collectable.transform.position = Vector3.Lerp(collectable.transform.position, transform.position, speed);
                }
            }
        }
        
        void OnDrawGizmos()
        {
            // Set the color of the Gizmos
            Gizmos.color = Color.yellow;
            
            // Draw a sphere to represent the magnet's radius
            Gizmos.DrawWireSphere(transform.position, magnetForce);
        }

    }
}

public class CollectableAnim : MonoBehaviour
{
    
    [Min(1)]
    public int collectableIndex;
}