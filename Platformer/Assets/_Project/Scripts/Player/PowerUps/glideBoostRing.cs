using System;
using UnityEngine;

namespace Platformer
{

    public class glideBoostRing : MonoBehaviour
    {
        public float glideBoostAmount = 2; // Amount of the glide boost
        public float boostDuration = 5f;  // Duration of the boost in seconds

        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController playerController))
            {
                if (playerController.glideBoost <= 1)
                {
                    Debug.Log("Glide Boost");
                    playerController.glideBoost = glideBoostAmount;
                    
                    // Start the timer to reset the boost
                    playerController.StartCoroutine(ResetGlideBoostAfterDuration(playerController));

                }
            }
        }
        
        private System.Collections.IEnumerator ResetGlideBoostAfterDuration(PlayerController playerController)
        {
            // Wait for the specified duration
            yield return new WaitForSeconds(boostDuration);

            // Reset the glide boost to its default value
            playerController.glideBoost = 1;
            Debug.Log("Glide Boost reset to default value");
        }

    }
}
