using UnityEngine;

namespace Platformer
{
    public class DeathState : BaseState
    {
        Health playerHealth;
        public DeathState(PlayerController player, Animator animator, Health playerHealth) : base(player, animator)
        {
            this.playerHealth = playerHealth;
        }
        public override void OnEnter()
        {
            animator.CrossFade(DieHash, crossFadeDuration);
            playerHealth.HandleDeath();
        }
        public override void FixedUpdate() { }
    }
}
