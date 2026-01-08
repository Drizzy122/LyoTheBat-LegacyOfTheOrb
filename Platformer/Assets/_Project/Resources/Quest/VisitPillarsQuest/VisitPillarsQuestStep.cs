using UnityEngine;
using System.Collections;

namespace Platformer
{
    public class VisitPillarsQuestStep : QuestStep
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float delayBeforeFinish = 2f; // Time in seconds before calling FinishQuestStep
        private void Start()
        {
            animator = GetComponent<Animator>();
        }
        private void OnTriggerEnter(Collider otherCollider)
        {
            if (otherCollider.CompareTag("Player"))
            {
               
                animator.SetBool("isNearby", true); // Play fade-out animation
                StartCoroutine(DelayedFinishQuestStep()); // Start a coroutine for delayed execution

            }
        }
        
        private IEnumerator DelayedFinishQuestStep()
        {
            
            yield return new WaitForSeconds(delayBeforeFinish); // Wait for the specified delay
            FinishQuestStep(); // Call FinishQuestStep after the delay
        }

        protected override void SetQuestStepState(string state)
        {
            // no state is needed for this quest step
        }
    }
}