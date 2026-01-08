using UnityEngine;

namespace Platformer
{
    public class FlyingEnemyChaseState : FlyingEnemyBaseState
    {
        public FlyingEnemyChaseState(FlyingEnemy enemy, Animator animator) : base(enemy, animator)
        {
        }

        public override void OnEnter()
        {
            animator.CrossFade(FlyHash, crossFadeDuration);
        }

        public override void Update()
        {
            enemy.HandleChase();
        }
    }
}