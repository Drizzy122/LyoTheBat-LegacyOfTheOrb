using UnityEngine;
using UnityEngine.AI;

namespace Platformer
{
    public class ChomperEnemyChaseState : ChomperEnemyBaseState
    {
        readonly NavMeshAgent agent;

        public ChomperEnemyChaseState(GroundAI groundAIEnemy, Animator animator, NavMeshAgent agent) : base(groundAIEnemy, animator)
        {
            this.agent = agent;
        }

        public override void OnEnter()
        {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
        }

        public override void Update()
        {
            if (GroundAIEnemy.playerDetector.Player == null) return;
            
            GroundAIEnemy.UpdateAnimation();
            agent.SetDestination(GroundAIEnemy.playerDetector.Player.position);
        }
    }
}