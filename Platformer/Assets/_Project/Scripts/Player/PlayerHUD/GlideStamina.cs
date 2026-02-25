using UnityEngine;

namespace Platformer
{
    public class GlideStamina : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float staminaRegenRate = 1f;
        
        [Header("Events")]
        [SerializeField] private FloatEventChannel staminaEventChannel; // Reference your StaminaEventChannel asset

        private float currentStamina;
        private bool isGliding;
        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            if (playerController != null)
                currentStamina = playerController.glideCoolDown;
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

            // Broadcast the current state to the Event Bus every frame it changes
            PublishStamina();
        }

        private void PublishStamina()
        {
            if (staminaEventChannel != null && playerController != null)
            {
                // Calculate and send the 0-1 percentage
                float fraction = currentStamina / playerController.glideCoolDown;
                staminaEventChannel.Invoke(fraction);
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