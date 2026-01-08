using UnityEngine;

namespace Platformer
{
    public class DoubleJumpState : BaseState
    {
        public DoubleJumpState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter()
        {
            // Play double jump animation
            animator.CrossFade(DoubleJumpHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            player.HandleJump();
            player.HandleMovement();
        }
    }
}
