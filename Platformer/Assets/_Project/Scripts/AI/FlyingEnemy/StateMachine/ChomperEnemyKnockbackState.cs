using UnityEngine;

namespace Platformer
{
    public class ChomperEnemyKnockbackState : ChomperEnemyBaseState
    {
        public ChomperEnemyKnockbackState(GroundAI groundAIEnemy, Animator animator) : base(groundAIEnemy, animator)
        {
        }

        public override void OnEnter()
        {
            GroundAIEnemy.agent.ResetPath();
            animator.CrossFade(KnockbackHash, crossFadeDuration);
        }
    }
}