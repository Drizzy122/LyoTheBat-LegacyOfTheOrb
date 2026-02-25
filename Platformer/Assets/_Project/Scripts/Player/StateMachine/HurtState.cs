using UnityEngine;

namespace Platformer 
{
    public class HurtState : BaseState 
    {
        public HurtState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter() 
        {
            animator.CrossFade(HurtHash, crossFadeDuration);
            player.StopMovement(); 
        }

        public override void FixedUpdate() 
        {
            // Intentionally empty! We don't call HandleMovement() so the player is stunned.
        }
    }
}