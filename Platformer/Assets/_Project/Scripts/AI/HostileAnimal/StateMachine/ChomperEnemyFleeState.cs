using UnityEngine;
using UnityEngine.AI;

namespace Platformer
{
    public class ChomperEnemyFleeState : ChomperEnemyBaseState
    {
        readonly NavMeshAgent agent;

        public ChomperEnemyFleeState(GroundAI groundAIEnemy, Animator animator, NavMeshAgent agent) : base(groundAIEnemy, animator)
        {
            this.agent = agent;
        }

        public override void OnEnter()
        {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
        }

        public override void Update()
        {
            GroundAIEnemy.UpdateAnimation();
            GroundAIEnemy.FleeFromPlayer();
        }

        public override void OnExit()
        {
            agent.speed = GroundAIEnemy.moveSpeed;
            GroundAIEnemy.isScared = false;
        }
    }
}