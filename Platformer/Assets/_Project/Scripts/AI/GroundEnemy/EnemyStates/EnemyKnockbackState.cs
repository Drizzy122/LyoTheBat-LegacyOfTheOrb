using UnityEngine;

namespace Platformer {
    public class EnemyKnockbackState : EnemyBaseState 
    {
        public EnemyKnockbackState(Enemy enemy, Animator animator) : base(enemy, animator) { }

        public override void OnEnter() 
        {
            animator.CrossFade(KnockBackHash, crossFadeDuration);
            
            // IMPORTANT: Stop pathfinding so we can push the agent manually
            enemy.Agent.ResetPath(); 
        }

        public override void Update() 
        {
            // Apply the sliding force we calculated in Enemy.cs
            enemy.HandleKnockbackPhysics();
        }
    }
}