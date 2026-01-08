using UnityEngine;

namespace Platformer
{
    public abstract class NpcBaseState : IState
    {
        protected readonly Npc npc;
        protected readonly Animator animator;
        
        protected static readonly int IdleHash = Animator.StringToHash("Idle");
        protected static readonly int WonderHash = Animator.StringToHash("Wonder");
        protected static readonly int InteractHash = Animator.StringToHash("Interact");
        
        protected const float crossFadeDuration = 0.1f;

        protected NpcBaseState(Npc npc, Animator animator)
        {
            this.npc = npc;
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