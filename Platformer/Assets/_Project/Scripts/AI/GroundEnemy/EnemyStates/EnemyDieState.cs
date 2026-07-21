using UnityEngine;

namespace Platformer
{
    public class EnemyDieState : EnemyBaseState
    {
        Health enemyHealth;
        public EnemyDieState(Enemy enemy, Animator animator, Health enemyHealth) : base(enemy, animator)
        {
            this.enemyHealth = enemyHealth;
        }

        public override void OnEnter()
        {
            enemy.AbortAttack();
            animator.CrossFade(DieHash, crossFadeDuration);
            enemyHealth.HandleDeath();
            
            // NEW: Tell the manager this enemy is dead and to remove it from the list
            if (EnemyManager.instance != null)
            {
                EnemyManager.instance.UnregisterEnemy(enemy);
            }
            
            enemy.GetComponent<Collider>().enabled = false;
            GameObject.Destroy(enemy.gameObject, 3f);
        }

        public override void Update() { }
    }
}