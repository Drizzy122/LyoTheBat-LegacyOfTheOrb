using UnityEngine;
using UnityEngine.AI;
using Utilities;
namespace Platformer
{
    public class NpcWonderState : NpcBaseState
    {
        readonly NavMeshAgent agent;
        readonly Vector3 startPoint;
        readonly float wanderRadius;
        readonly CountdownTimer wanderTimer;

        public NpcWonderState(Npc npc, Animator animator, NavMeshAgent agent, float wanderRadius, CountdownTimer wanderTimer
        ) : base(npc, animator)
        {
            this.agent = agent;
            this.startPoint = npc.transform.position;
            this.wanderRadius = wanderRadius;
            this.wanderTimer = wanderTimer;
        }
        public override void OnEnter()
        {
            animator.CrossFade(WonderHash, crossFadeDuration);
            agent.speed = 2f; // Slow speed for wandering
            wanderTimer.Start();

        }
        
        public override void Update()
        {
            // Check if the wander timer is complete, then transition back to idle
            if (!wanderTimer.IsRunning)
            {
                npc.ChangeStateToIdle();
                return;
            }

            if (HasReachedDestination())
            {
                var randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += startPoint;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
                var finalPosition = hit.position;

                agent.SetDestination(finalPosition);

                // Rotate smoothly towards the new direction
                Vector3 direction = (finalPosition - npc.transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, lookRotation, Time.deltaTime * npc.rotationSpeed);
            }
        }

        bool HasReachedDestination()
        {
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        }

    }
}