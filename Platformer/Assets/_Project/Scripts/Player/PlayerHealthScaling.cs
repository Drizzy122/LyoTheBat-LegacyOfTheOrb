using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Grows the player's max health as they level up (Spider-Man style).
    /// Max health = baseMaxHealth + (level - 1) * healthPerLevel.
    /// Lives on the Player next to Health.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerHealthScaling : MonoBehaviour
    {
        [SerializeField] float baseMaxHealth = 100f;
        [SerializeField] float healthPerLevel = 10f;

        Health health;

        void Awake() => health = GetComponent<Health>();

        void OnEnable()
        {
            GameEventsManager.instance.playerEvents.onPlayerLevelChange += OnLevelChanged;
        }

        void OnDisable()
        {
            if (GameEventsManager.instance != null)
                GameEventsManager.instance.playerEvents.onPlayerLevelChange -= OnLevelChanged;
        }

        void OnLevelChanged(int level)
        {
            health.SetMaxHealth(baseMaxHealth + (level - 1) * healthPerLevel);
        }
    }
}
