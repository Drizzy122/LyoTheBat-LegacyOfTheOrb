using UnityEngine;

namespace Platformer {
    public class AttackState : BaseState {
        public AttackState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter() {
            // 1. Get the next animation in the combo
            string animToPlay = player.combat.GetNextComboAnimation();
            
            // 2. Play that specific animation
            animator.CrossFade(animToPlay, crossFadeDuration);
            
            // 3. Pass the player's input direction into the new attack method!
            player.combat.LightAttack(player.GetAdjustedMovementDirection());
        }

        public override void FixedUpdate()
        {
            player.HandleMovement();
            // The player is now locked in place, and DOTween will handle the sliding.
        }
    }
}