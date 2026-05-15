using UnityEngine;

namespace Platformer
{
    public class NotificationTrigger : MonoBehaviour
    {
        [field: SerializeField] NotificationSO notificationData;
        

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (NotificationManager.instance != null)
                {
                    NotificationManager.instance.ShowNotification(notificationData);
                }

                Destroy(gameObject); // Optional for one time triggers
            }
        }
    }
}