using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Drives the fullscreen low-HP material from the player's health percentage.
    /// Invisible at/above startThreshold, ramps to full strength as health → 0.
    /// Assign the SAME material your URP Full Screen Pass Renderer Feature uses.
    /// </summary>
    public class LowHealthEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Material lowHpMaterial;   // FullScreenShader01_LowHP.mat
        [SerializeField] Health playerHealth;

        [Header("Settings")]
        [SerializeField] string intensityProperty = "_VignetteIntensity";
        [SerializeField, Range(0f, 1f)] float startThreshold = 0.3f;  // effect begins below this health %
        [SerializeField] float maxIntensity = 10f;                    // full-strength value (authored = 10)
        [SerializeField] float fadeSpeed = 8f;

        int intensityID;
        float current;

        void Awake() => intensityID = Shader.PropertyToID(intensityProperty);
        void Reset() => playerHealth = GetComponentInParent<Health>();

        void OnEnable() => SetIntensity(0f);   // start hidden
        void OnDisable() => SetIntensity(0f);  // reset so it doesn't linger on the shared material asset

        void Update()
        {
            if (playerHealth == null || lowHpMaterial == null) return;

            // 0 at/above the threshold, 1 at zero health
            float t = Mathf.InverseLerp(startThreshold, 0f, playerHealth.HealthPercent);
            float target = t * maxIntensity;

            SetIntensity(Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime));
        }

        void SetIntensity(float value)
        {
            current = value;
            if (lowHpMaterial != null) lowHpMaterial.SetFloat(intensityID, value);
        }
    }
}
