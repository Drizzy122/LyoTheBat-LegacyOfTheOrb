using UnityEngine;

namespace Platformer
{
    public class TriggerSave : MonoBehaviour
    {
        private DataPersistenceManager dataPersistenceManager;
        private Transform playerTransform;
        
        void Start()
        {
            dataPersistenceManager = FindFirstObjectByType<DataPersistenceManager>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Check if player is grounded before saving
                if (IsPlayerGrounded(other.transform))
                {
                    dataPersistenceManager.SaveGame();
                  //  print("Saved");
                }
            }
        }

        private bool IsPlayerGrounded(Transform player)
        {
            // Cast a small ray downward from the player's position
            float rayLength = 0.1f;
            RaycastHit hit;
            
            if (Physics.Raycast(player.position, Vector3.down, out hit, rayLength))
            {
                // Check if the object below is in the Ground layer
                return hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground");
            }
            
            return false;
        }
    }
}