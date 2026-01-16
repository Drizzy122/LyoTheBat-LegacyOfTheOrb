using UnityEngine;

namespace Platformer
{
    public class FlyingEnemyDieState : FlyingEnemyBaseState
    {
        private Health enemyHealth;
        public FlyingEnemyDieState(FlyingEnemy enemy, Animator animator, Health enemyHealth) : base(enemy, animator)
        {
            this.enemyHealth = enemyHealth;
          
        }

        public override void OnEnter()
        {
            animator.CrossFade(DieHash, crossFadeDuration);
            enemyHealth.HandleDeath();
            
            enemy.GetComponent<Collider>().enabled = false;
            GameObject.Destroy(enemy.gameObject, 3f);
        }
    }
}