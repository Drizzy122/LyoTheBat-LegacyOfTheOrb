using UnityEngine;
using UnityEngine.AI;

namespace Platformer
{
    public class ChomperEnemyWaitState : ChomperEnemyBaseState
    {
        readonly NavMeshAgent agent;

        public ChomperEnemyWaitState(GroundAI groundAIEnemy, Animator animator, NavMeshAgent agent) : base(groundAIEnemy, animator)
        {
            this.agent = agent;
        }

        public override void OnEnter()
        {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
            agent.ResetPath();
            
        }

        public override void Update()
        {
            GroundAIEnemy.FacePlayer();
            GroundAIEnemy.UpdateAnimation();
        }
    }
}