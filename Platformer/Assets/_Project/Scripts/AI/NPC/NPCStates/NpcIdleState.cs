using UnityEngine;
using UnityEngine.AI;
using ImprovedTimers;


namespace Platformer
{
    public class NpcIdleState : NpcBaseState
    {
        readonly NavMeshAgent agent;
        readonly Transform player;
        readonly CountdownTimer idleTimer;

        public NpcIdleState(Wanderer wanderer, Animator animator, NavMeshAgent agent, Transform player, CountdownTimer idleTimer) : base(wanderer, animator)
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
            agent.SetDestination(Wanderer.transform.position); // Stop moving
            Vector3 direction = (player.position - Wanderer.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            Wanderer.transform.rotation = Quaternion.Slerp(Wanderer.transform.rotation, lookRotation, Time.deltaTime * Wanderer.rotationSpeed);

            // Check if the timer has completed
            if (!idleTimer.IsRunning)
            {
                Wanderer.ChangeStateToWonder(); // This triggers the transition to the Wonder state
            }

        }
    }
}