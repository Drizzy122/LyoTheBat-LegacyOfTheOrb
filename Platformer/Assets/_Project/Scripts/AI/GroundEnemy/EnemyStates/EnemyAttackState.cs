using UnityEngine;

namespace Platformer {
    public class EnemyAttackState : EnemyBaseState {
        
        public EnemyAttackState(Enemy enemy, Animator animator) : base(enemy, animator) { }
        
        public override void OnEnter() {
            animator.CrossFade(AttackHash, crossFadeDuration);
        }
        
        public override void Update() {
            enemy.HandleAttack();
        }
    }
}