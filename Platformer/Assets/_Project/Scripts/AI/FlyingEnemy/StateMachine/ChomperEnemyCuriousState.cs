using UnityEngine;
using UnityEngine.AI;

namespace Platformer
{
    public class ChomperEnemyCuriousState : ChomperEnemyBaseState
    {
        readonly NavMeshAgent agent;

        public ChomperEnemyCuriousState(GroundAI groundAIEnemy, Animator animator, NavMeshAgent agent) : base(groundAIEnemy, animator)
        {
            this.agent = agent;
        }

        public override void OnEnter()
        {
            agent.ResetPath();
            animator.CrossFade(LocomotionHash, crossFadeDuration);
        }

        public override void Update()
        {
            GroundAIEnemy.FacePlayer();
            GroundAIEnemy.UpdateAnimation();
        }
    }
}