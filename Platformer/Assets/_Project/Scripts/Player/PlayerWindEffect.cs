using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Fades the fullscreen WIND material in/out based on whether the player
    /// is sprinting or gliding. Assign the SAME material that your URP
    /// Full Screen Pass Renderer Feature uses.
    /// </summary>
    public class PlayerWindEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Material windMaterial;        // WIND.mat — same instance the Full Screen Pass feature references
        [SerializeField] PlayerMovement playerMovement;

        [Header("Fade Settings")]
        [SerializeField, Range(0f, 1f)] float activeAlpha = 0.5f;
        [SerializeField] float fadeInSpeed = 6f;
        [SerializeField] float fadeOutSpeed = 4f;

        static readonly int AlphaID = Shader.PropertyToID("_Alpha");
        float currentAlpha;

        void Reset() => playerMovement = GetComponent<PlayerMovement>();

        void OnEnable() => SetAlpha(0f);   // start hidden
        void OnDisable() => SetAlpha(0f);  // reset so it doesn't linger on the shared material asset

        void Update()
        {
            if (playerMovement == null || windMaterial == null) return;

            bool active = playerMovement.IsSprintingActively || playerMovement.IsGliding;
            float target = active ? activeAlpha : 0f;
            float speed = target > currentAlpha ? fadeInSpeed : fadeOutSpeed;

            SetAlpha(Mathf.MoveTowards(currentAlpha, target, speed * Time.deltaTime));
        }

        void SetAlpha(float value)
        {
            currentAlpha = value;
            if (windMaterial != null) windMaterial.SetFloat(AlphaID, value);
        }
    }
}
