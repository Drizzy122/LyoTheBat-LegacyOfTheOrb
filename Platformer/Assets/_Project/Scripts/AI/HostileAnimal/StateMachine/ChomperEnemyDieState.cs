using UnityEngine;

namespace Platformer
{
    public class ChomperEnemyDieState : ChomperEnemyBaseState
    {
        readonly Health enemyHealth;

        public ChomperEnemyDieState(GroundAI groundAIEnemy, Animator animator, Health enemyHealth) : base(groundAIEnemy, animator)
        {
            this.enemyHealth = enemyHealth;
        }

        public override void OnEnter()
        {
            GroundAIEnemy.agent.ResetPath();
            animator.CrossFade(DieHash, crossFadeDuration);
            enemyHealth.HandleDeath();
            GroundAIEnemy.GetComponent<Collider>().enabled = false;
            //GameObject.Destroy(chomperEnemy.gameObject, 3f);
        }
    }
}