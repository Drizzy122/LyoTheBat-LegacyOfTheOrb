using UnityEngine;

namespace Platformer {
    public class EnemyAttackState : EnemyBaseState {
        
        public EnemyAttackState(Enemy enemy, Animator animator) : base(enemy, animator) { }
        
        public override void OnEnter() {
            animator.CrossFade(AttackHash, crossFadeDuration);

            // Trigger the lunge once!
            enemy.LungeAtPlayer();
        }
        
        public override void Update() {
            // We leave this empty now, because DOTween is handling the movement over time!
        }
    }
}