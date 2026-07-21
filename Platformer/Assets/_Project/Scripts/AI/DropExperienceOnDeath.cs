using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Spawns experience orbs around this entity when its Health reports death.
    /// Add to any enemy prefab (needs a Health component on the same GameObject)
    /// and assign the ExperienceOrb prefab.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class DropExperienceOnDeath : MonoBehaviour
    {
        [SerializeField] GameObject orbPrefab;
        [SerializeField, Min(1)] int orbCount = 3;
        [SerializeField] float scatterRadius = 1.2f;
        [SerializeField] float spawnHeight = 0.75f;

        Health health;

        void Awake() => health = GetComponent<Health>();

        void OnEnable()
        {
            if (health != null) health.OnDeath += Drop;
        }

        void OnDisable()
        {
            if (health != null) health.OnDeath -= Drop;
        }

        void Drop()
        {
            if (orbPrefab == null)
            {
                Debug.LogWarning($"{name}: DropExperienceOnDeath has no orb prefab assigned.", this);
                return;
            }

            for (int i = 0; i < orbCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * scatterRadius;
                Vector3 pos = transform.position + new Vector3(offset.x, spawnHeight, offset.y);
                Instantiate(orbPrefab, pos, Quaternion.identity);
            }
        }
    }
}
