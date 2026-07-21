using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Tracks the combat combo streak: +1 per enemy hit, reset to 0 when the player
    /// takes damage or when no hit lands for comboResetTime seconds.
    /// Broadcasts playerEvents.onComboChanged for the HUD. Lives on the Player.
    /// </summary>
    public class ComboTracker : MonoBehaviour
    {
        [Tooltip("Seconds without landing a hit before the streak resets.")]
        [SerializeField] float comboResetTime = 5f;

        int combo;
        float lastHitTime;

        void OnEnable()
        {
            var events = GameEventsManager.instance;
            events.enemyEvents.onEnemyHit += OnEnemyHit;
            events.playerEvents.onPlayerHit += OnPlayerHurt;
        }

        void OnDisable()
        {
            var events = GameEventsManager.instance;
            if (events == null) return;
            events.enemyEvents.onEnemyHit -= OnEnemyHit;
            events.playerEvents.onPlayerHit -= OnPlayerHurt;
        }

        void Update()
        {
            if (combo > 0 && Time.time - lastHitTime > comboResetTime)
                SetCombo(0);
        }

        void OnEnemyHit(float _)
        {
            lastHitTime = Time.time;
            SetCombo(combo + 1);
        }

        void OnPlayerHurt(float _) => SetCombo(0);

        void SetCombo(int value)
        {
            if (combo == value) return;
            combo = value;
            GameEventsManager.instance.playerEvents.ComboChanged(combo);
        }
    }
}
