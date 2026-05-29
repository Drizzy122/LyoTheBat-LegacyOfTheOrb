using UnityEngine;
using UnityEngine.AI;

namespace Platformer
{
    public class ChomperEnemyWanderState : ChomperEnemyBaseState
    {
        readonly NavMeshAgent agent;
        readonly Vector3 startPoint;
        readonly float wanderRadius;

        public ChomperEnemyWanderState(GroundAI groundAIEnemy, Animator animator, NavMeshAgent agent, float wanderRadius) : base(groundAIEnemy, animator)
        {
            this.agent = agent;
            this.startPoint = groundAIEnemy.transform.position;
            this.wanderRadius = wanderRadius;
        }

        public override void OnEnter()
        {
            agent.speed = GroundAIEnemy.moveSpeed;
            animator.CrossFade(LocomotionHash, crossFadeDuration);
        }

        public override void Update()
        {
            GroundAIEnemy.UpdateAnimation();

            if (HasReachedDestination()) {
                var randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += startPoint;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
                agent.SetDestination(hit.position);
            }
        }

        bool HasReachedDestination()
        {
            return !agent.pathPending
                   && agent.remainingDistance <= agent.stoppingDistance
                   && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        }
    }
}