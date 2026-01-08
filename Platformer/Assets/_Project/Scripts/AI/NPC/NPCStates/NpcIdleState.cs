using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace Platformer
{
    public class NpcIdleState : NpcBaseState
    {
        readonly NavMeshAgent agent;
        readonly Transform player;
        readonly CountdownTimer idleTimer;

        public NpcIdleState(Npc npc, Animator animator, NavMeshAgent agent, Transform player, CountdownTimer idleTimer) : base(npc, animator)
        {
            this.agent = agent;
            this.player = player;
            this.idleTimer = idleTimer;
        }

        public override void OnEnter()
        {
            animator.CrossFade(IdleHash, crossFadeDuration);
            idleTimer.Start();

        }
        public override void Update()
        {
            // NPC will look at the player while idling (optional)
            agent.SetDestination(npc.transform.position); // Stop moving
            Vector3 direction = (player.position - npc.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, lookRotation, Time.deltaTime * npc.rotationSpeed);

            // Check if the timer has completed
            if (!idleTimer.IsRunning)
            {
                npc.ChangeStateToWonder(); // This triggers the transition to the Wonder state
            }

        }
    }
}