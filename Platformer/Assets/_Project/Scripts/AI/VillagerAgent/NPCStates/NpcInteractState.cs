using UnityEngine;

namespace Platformer
{
    public class NpcInteractState : NpcBaseState
    {
        
        public NpcInteractState(Wanderer wanderer, Animator animator) : base(wanderer, animator)
        {
        }

        public override void OnEnter()
        {
            // Play the interact animation immediately upon entering the state
            animator.CrossFade(InteractHash, crossFadeDuration);
        }
        public override void Update()
        {
            
        }

       
    }
}