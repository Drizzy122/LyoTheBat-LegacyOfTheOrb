using UnityEngine;

namespace Platformer
{
    public class NpcInteractState : NpcBaseState
    {
        
        public NpcInteractState(Npc npc, Animator animator) : base(npc, animator)
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