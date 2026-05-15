using UnityEngine;

namespace Platformer
{
    public class SwordPickup : MonoBehaviour
    {
        [Tooltip("Drag the sword that is a child of the player here")]
        [SerializeField] GameObject equippedSword;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (other.TryGetComponent<PlayerCombat>(out var combat))
                combat.EquipWeapon();

            if (equippedSword != null)
                equippedSword.SetActive(true);

            Destroy(gameObject);
        }
    }
}
