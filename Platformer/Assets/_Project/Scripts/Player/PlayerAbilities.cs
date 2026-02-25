using UnityEngine;
using System.Collections;
using KBCore.Refs;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace Platformer
{
    public class PlayerAbilities : MonoBehaviour
    {
        #region Variables
        [field: Header("Events")]
        [SerializeField] FloatEventChannel sonarEventChannel;
       
        [field: Header("SonarPulse Settings")] 
        [SerializeField] LayerMask detectionLayer;
        [SerializeField] float detectionRadius = 30f; 
        [SerializeField] float detectionDuration = 3f;
        [SerializeField] int maxPulseCharges = 3;
        [SerializeField] float chargeRegenerationTime = 15f; 
        
        [field: Header("VFX Settings")]
        [SerializeField] ParticleSystem terrainScanPs;
        [SerializeField] ScriptableRendererFeature xRayRenderFeature;
        
        private int currentPulseCharges;
        private bool isRegenerating = false;
        public int CurrentPulseCharges => currentPulseCharges;
        public int MaxPulseCharges => maxPulseCharges;
        #endregion

        void Start()
        {
            currentPulseCharges = maxPulseCharges;
            PublishSonarCharges();
        }

        #region SonarPulse
        public void ExecuteSonarPulse()
        {
            if (currentPulseCharges <= 0) return;
            
            currentPulseCharges--;
            terrainScanPs.Play();
            PublishSonarCharges();
            
            Collider[] detectedObjects = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);
            foreach (Collider collider in detectedObjects)
            {
                if (collider.CompareTag("Enemy") || collider.CompareTag("Collectible"))
                {
                    EnableXrayEffect(true);
                    CancelInvoke(nameof(DisableXrayEffect));
                    Invoke(nameof(DisableXrayEffect), detectionDuration);
                    break; 
                }
            }
            if (!isRegenerating && currentPulseCharges < maxPulseCharges)
            {
                StartCoroutine(RegenerateUI());
            }
        }

        public void EnableXrayEffect(bool state)
        {
            if (xRayRenderFeature != null) xRayRenderFeature.SetActive(state);
        }

        private void DisableXrayEffect() => EnableXrayEffect(false);

        private IEnumerator RegenerateUI()
        {
            isRegenerating = true;
            float timer = 0;

            while (currentPulseCharges < maxPulseCharges)
            {
                timer += Time.deltaTime;
                
                // Calculate current fractional charge for smooth filling
                float smoothPercentage = ((float)currentPulseCharges / maxPulseCharges) + ((timer / chargeRegenerationTime) * (1f / maxPulseCharges));
                sonarEventChannel.Invoke(smoothPercentage); // Keep the UI updating smoothly
                
                if (timer >= chargeRegenerationTime)
                {
                    currentPulseCharges++;
                    timer = 0;
                    PublishSonarCharges();
                }
                yield return null;
            }
            isRegenerating = false;
        }
        public void PublishSonarCharges()
        {
            if (sonarEventChannel != null)
                sonarEventChannel.Invoke(currentPulseCharges / (float) maxPulseCharges);
        }
        #endregion

        #region OtherAbilities

        // TODO - Make the player have more abilities

        #endregion
        

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.lightBlue;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}