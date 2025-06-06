using UnityEngine;

namespace Platformer
{
    public class DeathState : BaseState
    {
        public DeathState(PlayerController player, Animator animator) : base(player, animator)
        {
            //noop
        }

        public override void OnEnter()
        {
            Debug.Log("Player is dead");
            animator.CrossFade(DieHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            //Noop
        }
    }
}
