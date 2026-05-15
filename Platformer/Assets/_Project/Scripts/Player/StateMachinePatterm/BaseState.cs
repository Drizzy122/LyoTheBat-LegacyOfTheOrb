using DG.Tweening;
using UnityEngine;
using UnityUtils.StateMachine;

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
        //protected static readonly int AttackHash = Animator.StringToHash("Attack");
        //protected static readonly int BlastAttackHash = Animator.StringToHash("SpinAttack");
        protected static readonly int GlideHash = Animator.StringToHash("Glide");
        protected static readonly int DieHash = Animator.StringToHash("Death");
        protected static readonly int TeleportationHash = Animator.StringToHash("Teleport");
        protected static readonly int SwimHash = Animator.StringToHash("Swim");
        protected static readonly int HurtHash = Animator.StringToHash("Hurt"); 

        protected const float crossFadeDuration = 0.1f;

        protected BaseState(PlayerController player, Animator animator)
        {
            this.player = player;
            this.animator = animator;
        }

        public virtual void OnEnter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void OnExit() { }
    }
    
    #region States
    public class LocomotionState : BaseState 
    {
        public LocomotionState(PlayerController player, Animator animator) : base(player, animator) { }
        public override void OnEnter() 
        {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
        }
        public override void FixedUpdate() 
        { 
            player.HandleMovement();
        }
    }
    public class SwimState : BaseState
    {
        public SwimState(PlayerController player, Animator animator) : base(player, animator) { }
        public override void OnEnter()
        {
            animator.CrossFade(SwimHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            player.HandleSwimming();
        }

        public override void OnExit() { }
    }
    public class WallClimbState : BaseState
    {
        public WallClimbState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter()
        {
            animator.CrossFade(WallclimbHash, crossFadeDuration);
            player.wallClimbPos = player.transform.position;
        }
        public override void FixedUpdate()
        {
            player.HandleWallClimb();
        }

        public override void OnExit()
        {
            player.wallClimbPos = Vector3.zero;
            player.wallClimbimg = false;
        }
    }
    public class DashState : BaseState
    {
        public DashState(PlayerController player, Animator animator) : base(player, animator) { }
        public override void OnEnter()
        {
            animator.CrossFade(DashHash, crossFadeDuration);
        }
        public override void FixedUpdate()
        {
            player.HandleMovement();
        }
    }
    public class GlideState : BaseState
    {
        public GlideState(PlayerController player, Animator animator) : base(player, animator) { }
        public override void OnEnter() {
            animator.CrossFade(GlideHash, crossFadeDuration);
        }
        public override void FixedUpdate() 
        {
            player.HandleGlide();
            player.HandleMovement();
        }
    }
    public class JumpState : BaseState 
    {
        public JumpState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter() {
            animator.CrossFade(JumpHash, crossFadeDuration);
        }

        public override void FixedUpdate() {
            player.HandleJump();
            player.HandleMovement();
        }
    }
    public class DoubleJumpState : BaseState
    {
        public DoubleJumpState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter()
        {
            // Play double jump animation
            animator.CrossFade(DoubleJumpHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            player.HandleJump();
            player.HandleMovement();
        }
    }
    public class AttackState : BaseState {
        public AttackState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter() {
            player.transform.DOKill();

            string anim = player.commandManager.LightAttackCommand.Advance();
            animator.CrossFade(anim, crossFadeDuration);
            _ = player.commandManager.LightAttackCommand.Execute();
        }

        public override void FixedUpdate()
        {
            player.HandleMovement();
        }
    }
    public class BlastAttackState : BaseState
    {
        public BlastAttackState(PlayerController player, Animator animator) : base(player, animator) { }
        public override void OnEnter()
        {
            // Kill any lingering DOTween tweens (e.g. DOLookAt/DOMove from light attack)
            player.transform.DOKill();

            string anim = player.commandManager.BlastAttackCommand.Advance();
            animator.CrossFade(anim, crossFadeDuration);
            _ = player.commandManager.BlastAttackCommand.Execute();
        }
        public override void FixedUpdate() { }
    }
    public class HurtState : BaseState 
    {
        public HurtState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter() 
        {
            animator.CrossFade(HurtHash, crossFadeDuration);
            player.StopMovement(); 
        }
        public override void FixedUpdate() { }
    }
    public class DeathState : BaseState
    {
        Health playerHealth;
        public DeathState(PlayerController player, Animator animator, Health playerHealth) : base(player, animator)
        {
            this.playerHealth = playerHealth;
        }
        public override void OnEnter()
        {
            animator.CrossFade(DieHash, crossFadeDuration);
            playerHealth.HandleDeath();
        }
        public override void FixedUpdate() { }
    }
    public class TeleportState : BaseState
    {
        public TeleportState(PlayerController player, Animator animator) : base(player, animator) { }
        
        public override void OnEnter() {
            animator.CrossFade(TeleportationHash, crossFadeDuration);
        }
        public override void FixedUpdate() 
        {
            
        }
    }
    #endregion
}