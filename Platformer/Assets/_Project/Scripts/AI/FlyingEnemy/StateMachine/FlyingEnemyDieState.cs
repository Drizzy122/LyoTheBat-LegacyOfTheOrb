using UnityEngine;

namespace Platformer
{
    public class FlyingEnemyDieState : FlyingEnemyBaseState
    {
        private EnemyHealth enemyHealth;
        public FlyingEnemyDieState(FlyingEnemy enemy, Animator animator, EnemyHealth enemyHealth) : base(enemy, animator)
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