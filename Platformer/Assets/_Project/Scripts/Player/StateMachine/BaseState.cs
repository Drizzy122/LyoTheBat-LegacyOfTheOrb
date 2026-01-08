using UnityEngine;

namespace Platformer
{
    public abstract class BaseState : IState   
    {
        protected readonly PlayerController player;
        protected readonly Animator animator;

        protected static readonly int DoubleJumpHash = Animator.StringToHash("DoubleJump");
        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int JumpHash = Animator.StringToHash("Jump");
        protected static readonly int WallclimbHash = Animator.StringToHash("WallClimb");
        protected static readonly int DashHash = Animator.StringToHash("Dash");
        protected static readonly int AttackHash = Animator.StringToHash("Attack");
        protected static readonly int SpinAttackHash = Animator.StringToHash("SpinAttack");
        protected static readonly int GlideHash = Animator.StringToHash("Glide");
        protected static readonly int DieHash = Animator.StringToHash("Death");
        protected static readonly int EcholocationHash = Animator.StringToHash("Echolocation");        
        protected static readonly int TeleportationHash = Animator.StringToHash("Teleport");
        
        // new states for the player
        protected static readonly int FallHash = Animator.StringToHash("Falling");
        protected static readonly int LandHash = Animator.StringToHash("Land");
        protected static readonly int DrowningHash = Animator.StringToHash("Drowning");

        protected const float crossFadeDuration = 0.1f;

        protected BaseState(PlayerController player, Animator animator)
        {
            this.player = player;
            this.animator = animator;
        }

        public virtual void OnEnter()
        {
            // noop
        }

        public virtual void Update()
        {
            // noop
        }

        public virtual void FixedUpdate()
        {
            // noop
        }

        public virtual void OnExit()
        {
            // noop
        }
    }
}