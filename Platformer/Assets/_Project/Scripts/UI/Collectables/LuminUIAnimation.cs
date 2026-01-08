using System.Collections;
using UnityEngine;

namespace Platformer
{
    
    public class LuminUIAnimation : MonoBehaviour
    {
        private Animator animator;
        bool collected = false;
        public float duration = 5f;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        
        }

        private void OnEnable()
        {
            GameEventsManager.instance.miscEvents.onLuminCollected += LuminCollected;
            
        }

        private void OnDisable()
        {
            GameEventsManager.instance.miscEvents.onLuminCollected -= LuminCollected;
            
        }

        private void LuminCollected()
        {
            if (!collected)
            {
                collected = true;
                animator.SetBool("show", true);
                StartCoroutine(ResetCollected());
            }
        }
        
        private IEnumerator ResetCollected()
        {
            yield return new WaitForSeconds(duration); // Adjust the delay to match your animation length
            collected = false;
            animator.SetBool("show", false); // Reset animator parameter if needed
        }
    }
}