using UnityEngine;

namespace Platformer
{
    public class GlideStamina : MonoBehaviour
    {
        [SerializeField] private float staminaRegenRate = 1f;

        private float currentStamina;
        private bool isGliding;
        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            if (playerController != null)
                currentStamina = playerController.glideTime;
        }

        private void Update()
        {
            if (isGliding)
            {
                DrainStamina(); // Drain stamina when gliding
            }
            else
            {
                RegenerateStamina(); // Regenerate stamina when not gliding
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
            isGliding = false; // Stop draining stamina
        }

        private void DrainStamina()
        {
            if (currentStamina > 0f && playerController != null)
            {
                currentStamina -= Time.deltaTime; // Drain stamina over time
                currentStamina = Mathf.Clamp(currentStamina, 0f, playerController.glideTime);

                // Automatically stop glide if stamina is less then 0
                if (currentStamina <= 0f)
                {
                    StopGlideExternally();
                }
            }
        }

        private void RegenerateStamina()
        {
            if (currentStamina < playerController.glideTime) // Use controller's `glideTime` as max value
            {
                currentStamina += staminaRegenRate * Time.deltaTime; // Gradually restore stamina
                currentStamina = Mathf.Clamp(currentStamina, 0f, playerController.glideTime);
            }
        }

        // Stops glide externally, ensuring both stamina and timer synchronization
        private void StopGlideExternally()
        {
            StopGlide();

            // Notify the PlayerController to stop the glide (if needed)
            if (playerController != null)
            {
                playerController.OnGlide(false); // Stops the glide from PlayerController
            }
        }

        public float GetStaminaFraction()
        {
            return currentStamina / (playerController != null ? playerController.glideTime : 1f);
        }
    }
}