using UnityEngine;

namespace Platformer
{
    public class SwordPickup : MonoBehaviour, IDataPersistence
    {
        [Tooltip("Drag the sword that is a child of the player here")]
        [SerializeField] GameObject equippedSword;

        [SerializeField] private string id;
        private bool isPickedUp = false;

        [ContextMenu("Generate guid for id")]
        private void GenerateGuid() => id = System.Guid.NewGuid().ToString();

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            PlayerCombat combat = null;
            other.TryGetComponent(out combat);

            PickupSword(combat);
        }

        private void PickupSword(PlayerCombat combat)
        {
            isPickedUp = true;

            if (combat != null)
                combat.EquipWeapon();

            if (equippedSword != null)
                equippedSword.SetActive(true);

            Destroy(gameObject);
        }

        public void LoadData(GameData data)
        {
            if (data.hasSword)
            {
                isPickedUp = true;

                // Re-equip the sword without needing the trigger collision
                PlayerCombat combat = FindObjectOfType<PlayerCombat>();
                if (combat != null) combat.EquipWeapon();

                if (equippedSword != null) equippedSword.SetActive(true);

                Destroy(gameObject);
            }
        }

        public void SaveData(GameData data)
        {
            // Once picked up this stays true — never overwrite true with false
            if (isPickedUp) data.hasSword = true;
        }
    }
}
