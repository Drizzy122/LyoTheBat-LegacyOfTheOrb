using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Platformer
{
    public class PlayerAbilities : MonoBehaviour
    {
        [field: Header("Equipped Ability")]
        // Instead of hardcoding Sonar Pulse, you drag ANY ability Strategy here!
        [SerializeField] private AbilityStrategy activeAbility; 
        
        private float nextAbilityTime;

        // This is called from your PlayerController or Input system
        public void ExecuteActiveAbility()
        {
            if (activeAbility != null && Time.time >= nextAbilityTime)
            {
                activeAbility.Execute(this.gameObject);
                nextAbilityTime = Time.time + activeAbility.cooldownTime; // Automatically handles cooldowns!
            }
            else if (activeAbility == null)
            {
                Debug.LogWarning("No ability equipped!");
            }
        }

        #region X-Ray Helper 
        // We leave this helper here because MonoBehaviour handles Invoke/Coroutines better than ScriptableObjects
        private ScriptableRendererFeature currentXRay;

        public void TriggerXRay(ScriptableRendererFeature feature, float duration)
        {
            if (feature == null) return;
            
            currentXRay = feature;
            currentXRay.SetActive(true);
            
            CancelInvoke(nameof(DisableXRay));
            Invoke(nameof(DisableXRay), duration);
        }

        private void DisableXRay()
        {
            if (currentXRay != null) currentXRay.SetActive(false);
        }
        #endregion
    }
}