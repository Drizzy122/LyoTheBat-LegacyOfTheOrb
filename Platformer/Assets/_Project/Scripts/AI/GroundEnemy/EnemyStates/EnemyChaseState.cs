using UnityEngine;

namespace Platformer {
    public class EnemyChaseState : EnemyBaseState 
    {
        public EnemyChaseState(Enemy enemy, Animator animator) : base(enemy, animator) { }
        
        public override void OnEnter() {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
            if (EnemyManager.instance != null) EnemyManager.instance.RegisterEnemy(enemy);
        }
        
        public override void Update() {
            enemy.DoChase();
            enemy.UpdateAnimation();
        }
    }
}