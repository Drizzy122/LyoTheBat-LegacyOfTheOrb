using UnityEngine;

namespace Platformer {
    public class EnemyChaseState : EnemyBaseState 
    {
        public EnemyChaseState(Enemy enemy, Animator animator) : base(enemy, animator) { }
        
        public override void OnEnter() 
        {
            animator.CrossFade(RunHash, crossFadeDuration);
        }
        
        public override void Update() {
            enemy.HandleChase();
        }
    }
}