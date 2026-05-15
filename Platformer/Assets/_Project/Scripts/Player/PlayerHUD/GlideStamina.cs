using UnityEngine;
using UnityEngine.UIElements; // Required for UI Toolkit

namespace Platformer
{
    public class GlideStamina : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float staminaRegenRate = 1f;
        
        [Header("Events")]
        [SerializeField] private FloatEventChannel staminaEventChannel; // Kept in case you still want to broadcast

        [Header("UI Toolkit Integration")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string targetElementID = "StaminaBar";
        [Tooltip("If true, the container will fade out when the bar reaches 100%.")]
        [SerializeField] private bool hideWhenFull = true;
        [SerializeField] private string containerElementID = "StaminaContainer";

        private float currentStamina;
        private bool isGliding;
        private PlayerController playerController;

        // UI Toolkit Variables
        private VisualElement barFill;
        private VisualElement barContainer;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            if (playerController != null)
                currentStamina = playerController.glideCoolDown;
        }

        private void Start()
        {
            // Query the UI elements when the script starts
            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                barFill = root.Q<VisualElement>(targetElementID);

                if (hideWhenFull)
                {
                    barContainer = root.Q<VisualElement>(containerElementID);
                }
            }
            else
            {
                Debug.LogWarning("UIDocument is not assigned in GlideStamina!");
            }
        }

        private void Update()
        {
            if (isGliding)
            {
                DrainStamina();
            }
            else
            {
                RegenerateStamina();
            }

            // Broadcast and update UI every frame it changes
            PublishStamina();
        }

        private void PublishStamina()
        {
            float fraction = 0f;
            if (playerController != null && playerController.glideCoolDown > 0f)
            {
                // Calculate the 0-1 percentage
                fraction = currentStamina / playerController.glideCoolDown;
            }

            // 1. Broadcast to Event Bus (Optional, kept from your original script)
            if (staminaEventChannel != null)
            {
                staminaEventChannel.Invoke(fraction);
            }

            // 2. Direct UI Toolkit Update
            if (barFill != null)
            {
                // Snap the width (Rely on USS transitions for smoothing)
                barFill.style.width = new Length(fraction * 100f, LengthUnit.Percent);
            }

            // 3. Handle the fade out when full
            if (hideWhenFull && barContainer != null)
            {
                barContainer.style.opacity = fraction >= 0.99f ? 0f : 1f;
            }
        }

        public void StartGlide()
        {
            if (playerController != null && currentStamina > 0f)
            {
                isGliding = true;
            }
        }

        public void StopGlide()
        {
            isGliding = false;
        }

        private void DrainStamina()
        {
            if (currentStamina > 0f && playerController != null)
            {
                currentStamina -= Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0f, playerController.glideCoolDown);

                if (currentStamina <= 0f)
                {
                    StopGlideExternally();
                }
            }
        }

        private void RegenerateStamina()
        {
            if (playerController != null && currentStamina < playerController.glideCoolDown)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0f, playerController.glideCoolDown);
            }
        }

        private void StopGlideExternally()
        {
            StopGlide();
            if (playerController != null)
            {
                playerController.OnGlide(false);
            }
        }
    }
}