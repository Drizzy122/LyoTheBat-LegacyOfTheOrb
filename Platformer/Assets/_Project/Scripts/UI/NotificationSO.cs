using UnityEngine;

namespace Platformer
{
    [CreateAssetMenu(fileName = "NotificationSO", menuName = "ScriptableObjects/NotificationSO")]
    public class NotificationSO : ScriptableObject
    {
        [SerializeField, TextArea] private string message;
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeDuration = 1f;

        public string Message => message;
        public float DisplayDuration => displayDuration;
        public float FadeDuration => fadeDuration;
    }
}