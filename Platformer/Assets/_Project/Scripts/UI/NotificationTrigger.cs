using UnityEngine;

namespace Platformer
{
    public class NotificationTrigger : MonoBehaviour, IDataPersistence
    {
        [SerializeField] NotificationSO notificationData;

        [SerializeField] private string id;
        private bool hasTriggered = false;

        [ContextMenu("Generate guid for id")]
        private void GenerateGuid() => id = System.Guid.NewGuid().ToString();

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            hasTriggered = true;

            if (NotificationManager.instance != null)
                NotificationManager.instance.ShowNotification(notificationData);

            Destroy(gameObject);
        }

        public void LoadData(GameData data)
        {
            data.notificationsTriggered.TryGetValue(id, out hasTriggered);

            if (hasTriggered)
                Destroy(gameObject); // Already seen — remove silently, no notification
        }

        public void SaveData(GameData data)
        {
            if (data.notificationsTriggered.ContainsKey(id))
                data.notificationsTriggered.Remove(id);

            data.notificationsTriggered.Add(id, hasTriggered);
        }
    }
}
