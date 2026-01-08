using UnityEngine;

namespace Platformer
{
    public class SpinAttackState : BaseState
    {
        
        public SpinAttackState(PlayerController player, Animator animator) : base(player, animator)
        {
        }
        
        public override void OnEnter()
        {
           // Debug.Log("Player is Attacking");
            animator.CrossFade(SpinAttackHash, crossFadeDuration);
            player.SpinAttack();
        }
        public override void FixedUpdate()
        {
            player.HandleMovement();
        }
    }
}
