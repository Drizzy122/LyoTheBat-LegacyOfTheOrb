using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    public class HealthBar : MonoBehaviour
    {
        [field: SerializeField] Image healthBar;
        [field: SerializeField] float lerpSpeed = 5f;
        private float targetFill = 1f;
        
        public void UpdateTargetFill(float percentage) => targetFill = percentage;
        private void Update() => healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, targetFill, lerpSpeed * Time.deltaTime);
    }
}