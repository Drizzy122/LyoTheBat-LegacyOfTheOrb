using UnityEngine;

namespace Platformer
{
    public class Traps : MonoBehaviour
    {
        [SerializeField] private float damage;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<Health>().TakeDamage(damage);
            }
        }
    }
}
