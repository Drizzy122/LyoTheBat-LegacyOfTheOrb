using UnityEngine;

namespace Platformer {
    public class EnemyWanderState : EnemyBaseState 
    {
        public EnemyWanderState(Enemy enemy, Animator animator) : base(enemy, animator) { }
        
        public override void OnEnter() 
        {
            animator.CrossFade(WalkHash, crossFadeDuration);
        }

        public override void Update() 
        {
            enemy.HandleWander();
        }
    }
}