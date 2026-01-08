using UnityEngine;

namespace Platformer
{
    public class EnemyDieState : EnemyBaseState
    {
        EnemyHealth enemyHealth;
        public EnemyDieState(Enemy enemy, Animator animator, EnemyHealth enemyHealth) : base(enemy, animator)
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

        public override void Update() { }
    }
}
