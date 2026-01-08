using UnityEngine;

namespace Platformer
{
    public class FlyingEnemyKnockbackState : FlyingEnemyBaseState
    {
        public FlyingEnemyKnockbackState(FlyingEnemy enemy, Animator animator) : base(enemy, animator)
        {
        }

        public override void OnEnter()
        {
            animator.CrossFade(KnockBackHash, crossFadeDuration);
        }

        public override void Update()
        {
            enemy.HandleKnockback();
        }
    }
}