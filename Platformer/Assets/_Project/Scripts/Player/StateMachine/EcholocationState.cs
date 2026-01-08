using UnityEngine;

namespace Platformer
{
    public class EcholocationState : BaseState
    {
        public EcholocationState(PlayerController player, Animator animator) : base(player, animator)
        {
            //Noop
        }

        public override void OnEnter()
        {
            //Debug.Log("player is now in Echolocation State");
            animator.CrossFade(EcholocationHash, crossFadeDuration);
        }
        public override void Update()
        {
            player.HandleEcho();
        }
    }
}
