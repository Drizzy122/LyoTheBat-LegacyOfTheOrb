using UnityEngine;

namespace Platformer {
    public class EnemyWanderState : EnemyBaseState 
    {
        public EnemyWanderState(Enemy enemy, Animator animator) : base(enemy, animator) { }
        
        public override void OnEnter() {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
            if (EnemyManager.instance != null) EnemyManager.instance.UnregisterEnemy(enemy);
        }

        public override void Update() {
            enemy.DoWander();
            enemy.UpdateAnimation();
        }
    }
}