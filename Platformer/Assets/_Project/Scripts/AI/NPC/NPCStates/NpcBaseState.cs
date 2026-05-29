using UnityEngine;
using UnityUtils.StateMachine;

namespace Platformer
{
    public abstract class NpcBaseState : IState
    {
        protected readonly Wanderer Wanderer;
        protected readonly Animator animator;
        
        protected static readonly int IdleHash = Animator.StringToHash("Idle");
        protected static readonly int WonderHash = Animator.StringToHash("Wonder");
        protected static readonly int InteractHash = Animator.StringToHash("Interact");
        
        protected const float crossFadeDuration = 0.1f;

        protected NpcBaseState(Wanderer wanderer, Animator animator)
        {
            this.Wanderer = wanderer;
            this.animator = animator;
        }
        public virtual void OnEnter() {
            // noop
        }

        public virtual void Update() {
            // noop
        }

        public virtual void FixedUpdate() {
            // noop
        }

        public virtual void OnExit() {
            // noop
        }
    }
}