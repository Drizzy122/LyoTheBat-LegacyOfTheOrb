using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Chance-based pickup drops when this entity's Health reports death —
    /// the loot half of the combat loop (XP is handled by DropExperienceOnDeath).
    /// Same composition pattern: add to any enemy, fill the drop table with
    /// Collectable-based pickup prefabs (potions, coins, items...).
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class DropItemOnDeath : MonoBehaviour
    {
        [System.Serializable]
        public class Drop
        {
            [Tooltip("A pickup prefab with a Collectable component (item, coin, health...).")]
            public GameObject pickupPrefab;

            [Tooltip("0 = never, 1 = always.")]
            [Range(0f, 1f)] public float chance = 0.35f;
        }

        [SerializeField] Drop[] drops;
        [SerializeField] float scatterRadius = 1.2f;
        [SerializeField] float spawnHeight = 0.75f;

        Health health;

        void Awake() => health = GetComponent<Health>();

        void OnEnable()
        {
            if (health != null) health.OnDeath += DropLoot;
        }

        void OnDisable()
        {
            if (health != null) health.OnDeath -= DropLoot;
        }

        void DropLoot()
        {
            if (drops == null) return;

            foreach (Drop drop in drops)
            {
                if (drop == null || drop.pickupPrefab == null) continue;
                if (Random.value > drop.chance) continue;

                Vector2 offset = Random.insideUnitCircle * scatterRadius;
                Vector3 pos = transform.position + new Vector3(offset.x, spawnHeight, offset.y);
                GameObject pickup = Instantiate(drop.pickupPrefab, pos, Quaternion.identity);

                // Fresh save-id so runtime drops never clash with authored pickups
                var collectable = pickup.GetComponent<Collectable>();
                if (collectable != null) collectable.InitAsRuntimeDrop();
            }
        }
    }
}
