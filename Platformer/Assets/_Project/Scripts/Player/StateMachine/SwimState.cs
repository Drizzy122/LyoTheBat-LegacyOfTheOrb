using UnityEngine;

namespace Platformer
{
    public class SwimState : BaseState
    {
        public SwimState(PlayerController player, Animator animator) : base(player, animator) { }
        public override void OnEnter()
        {
            animator.CrossFade(SwimHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            // Use the specific swimming logic, not standard ground movement
            player.HandleSwimming();
        }

        public override void OnExit()
        {
          
        }
        
        
    }
}