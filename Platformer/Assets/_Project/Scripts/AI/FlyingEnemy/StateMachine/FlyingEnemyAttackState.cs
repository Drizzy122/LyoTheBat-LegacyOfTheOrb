using UnityEngine;

namespace Platformer
{
    public class FlyingEnemyAttackState : FlyingEnemyBaseState
    {
        public FlyingEnemyAttackState(FlyingEnemy enemy, Animator animator) : base(enemy, animator)
        {
        }

        public override void OnEnter()
        {
            animator.CrossFade(AttackHash, crossFadeDuration);
        }

        public override void Update()
        {
            enemy.HandleAttack();
        }
    }
}