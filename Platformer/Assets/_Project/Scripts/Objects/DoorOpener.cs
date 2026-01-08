using UnityEngine;

namespace Platformer
{
    public class DoorOpener : MonoBehaviour
    {
        public GameObject door;
        public float doorOpenDuration = 2f;
        public Vector3 doorOpenVector = new Vector3(0, -9, 0);
        

        private bool isOpening = false;
        private bool isFullyOpen = false;
        private float openStartTime = 0f;
        private Vector3 doorStartPosition;

        public int amountToKill = 9;
        private int currentKillCount = 0; // Counts the player kills

        private void Start()
        {
            doorStartPosition = door.transform.position;

            // Subscribe to the onEnemyDeath event
            GameEventsManager.instance.enemyEvents.onEnemyDeath += OnEnemyDeath;
        }

        private void OnDestroy()
        {
            // Unsubscribe from the onEnemyDeath event when the object is destroyed
            GameEventsManager.instance.enemyEvents.onEnemyDeath -= OnEnemyDeath;
        }

        private void Update()
        {
            if (isOpening)
            {
                float openTimeElapsed = Time.time - openStartTime;
                if (openTimeElapsed <= doorOpenDuration)
                {
                    float doorProgress = openTimeElapsed / doorOpenDuration;
                    door.transform.position = doorStartPosition + doorOpenVector * doorProgress;
                }
                else
                {
                    door.transform.position = doorStartPosition + doorOpenVector; // Ensure it's fully opened
                    isOpening = false;
                    isFullyOpen = true; // Mark the door as fully open
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isFullyOpen && !isOpening && currentKillCount >= amountToKill)
            {
                Debug.Log("Opening door!");
                isOpening = true;
                openStartTime = Time.time;
            }
            else 
            {
                Debug.Log("Door not opened!");
                isOpening = false;
            }
        }

        private void OnEnemyDeath()
        {
            currentKillCount++; // Increment the kill count on enemy death
        }
    }
}