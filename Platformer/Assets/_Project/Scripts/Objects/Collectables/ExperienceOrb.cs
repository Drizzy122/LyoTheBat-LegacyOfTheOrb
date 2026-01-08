using Unity.VisualScripting;
using UnityEngine;

namespace Platformer
{
    public class ExperienceOrb : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int experienceGained = 25;

        private SphereCollider sphereCollider;
        private void Awake() 
        {
            sphereCollider = GetComponent<SphereCollider>();
        }
        private void CollectExperience() 
        {
            sphereCollider.enabled = false;
            gameObject.SetActive(false);
            GameEventsManager.instance.playerEvents.ExperienceGained(experienceGained);
            GameEventsManager.instance.miscEvents.XPCollected();
        }
        private void OnTriggerEnter(Collider otherCollider) 
        {
            if (otherCollider.CompareTag("Player"))
            {
                CollectExperience();
                Destroy(gameObject,6f);
            }
        }
    }
}