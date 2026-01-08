using UnityEngine;
using UnityEngine.VFX;

namespace Platformer
{
    public class PowerUp : MonoBehaviour
    {
        public VisualEffect swordFx;
        public bool isSwordFXActive = false; // Independent state for swordFX
        public VisualEffect bodyFx;
        public bool isBodyFXActive = false; // Independent state for bodyFx
        public float effectTimer = 10f;

        private void Awake()
        {
            swordFx.Stop(); // Stop sword visual effect on startup
            bodyFx.Stop(); // Stop body visual effect on startup
        }

        private void OnEnable()
        {
            GameEventsManager.instance.miscEvents.onLuminCollected += ActivateEffect;
        }

        private void OnDisable()
        {
            GameEventsManager.instance.miscEvents.onLuminCollected -= ActivateEffect;
        }

        void ActivateEffect()
        {
            if (swordFx != null && !isSwordFXActive)
            {
                swordFx.Play(); // Start the sword visual effect
                isSwordFXActive = true;
                Invoke(nameof(DeactivateSwordEffect), effectTimer);
            }

            if (bodyFx != null && !isBodyFXActive)
            {
                bodyFx.Play(); // Start the body visual effect
                isBodyFXActive = true;
                Invoke(nameof(DeactivateBodyEffect), effectTimer);
            }
        }

        void DeactivateSwordEffect()
        {
            if (swordFx != null && isSwordFXActive)
            {
                swordFx.Stop();
                isSwordFXActive = false;
            }
        }

        void DeactivateBodyEffect()
        {
            if (bodyFx != null && isBodyFXActive)
            {
                bodyFx.Stop();
                isBodyFXActive = false;
            }
        }
    }
}