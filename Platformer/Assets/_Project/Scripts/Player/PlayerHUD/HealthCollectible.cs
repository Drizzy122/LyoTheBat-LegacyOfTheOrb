using UnityEngine;

namespace Platformer
{
    public class HealthCollectible : MonoBehaviour
    {
        [SerializeField] private float healthValue;

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.tag == "Player")
            {
                collision.GetComponent<Health>().AddHealth(healthValue);
                gameObject.SetActive(false);
            }
        }
    }
}