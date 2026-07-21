using UnityEngine;
using UnityUtils.StateMachine;

namespace Platformer
{
    public abstract class ChomperEnemyBaseState : IState
    {
        protected readonly GroundAI GroundAIEnemy;
        protected readonly Animator animator;

        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int KnockbackHash = Animator.StringToHash("Hit");
        protected static readonly int DieHash = Animator.StringToHash("Die");

        protected const float crossFadeDuration = 0.1f;

        protected ChomperEnemyBaseState(GroundAI groundAIEnemy, Animator animator)
        {
            this.GroundAIEnemy = groundAIEnemy;
            this.animator = animator;
        }

        public virtual void OnEnter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void OnExit() { }
    }
}