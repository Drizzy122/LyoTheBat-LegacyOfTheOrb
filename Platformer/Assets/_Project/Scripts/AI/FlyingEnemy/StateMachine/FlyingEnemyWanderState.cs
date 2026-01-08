using UnityEngine;

namespace Platformer
{
    public class FlyingEnemyWanderState : FlyingEnemyBaseState
    {
        public FlyingEnemyWanderState(FlyingEnemy enemy, Animator animator) : base(enemy, animator)
        {
        }

        public override void OnEnter() 
        {
            animator.CrossFade(FlyHash, crossFadeDuration);
        }

        // IMPORTANT: This calls the logic in FlyingEnemy.cs
        // If this is missing or empty, the enemy does nothing!
        public override void FixedUpdate()
        {
            enemy.HandleWander();
        }
    }
}