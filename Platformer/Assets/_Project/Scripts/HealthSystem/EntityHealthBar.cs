using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    // World-space health bar that tracks its own entity's Health directly.
    // The shared FloatEventChannel is for the singleton player HUD — routing
    // per-enemy bars through it made every bar show the last-damaged enemy.
    public class EntityHealthBar : MonoBehaviour
    {
        [field: Header("References")]
        [SerializeField] Health health;
        [SerializeField] Image fillImage;
        [SerializeField] float lerpSpeed = 5f;

        float currentFill = 1f;

        void Awake()
        {
            if (health == null) health = GetComponentInParent<Health>();
        }

        void Update()
        {
            if (health == null || fillImage == null) return;
            currentFill = Mathf.Lerp(currentFill, health.HealthPercent, lerpSpeed * Time.deltaTime);
            fillImage.fillAmount = currentFill;
        }
    }
}
