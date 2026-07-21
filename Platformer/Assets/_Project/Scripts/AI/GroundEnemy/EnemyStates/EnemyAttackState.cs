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

        public override void OnExit() {
            // Knockback/death can interrupt mid-lunge — tear down the tween chain so a
            // dead or stunned enemy can't keep sliding and land the hit anyway.
            enemy.AbortAttack();
        }
    }
}