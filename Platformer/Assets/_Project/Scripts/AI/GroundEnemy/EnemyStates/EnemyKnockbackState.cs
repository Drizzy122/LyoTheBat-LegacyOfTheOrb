using UnityEngine;

namespace Platformer {
    public class EnemyKnockbackState : EnemyBaseState 
    {
        public EnemyKnockbackState(Enemy enemy, Animator animator) : base(enemy, animator) { }

        public override void OnEnter() {
            animator.CrossFade(KnockBackHash, crossFadeDuration);
        }

        public override void Update() {
            enemy.HandleKnockbackPhysics();
        }
    }
}