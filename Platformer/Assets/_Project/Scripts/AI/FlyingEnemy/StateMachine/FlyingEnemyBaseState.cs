using UnityEngine;
using UnityUtils.StateMachine;

namespace Platformer
{
    public abstract class FlyingEnemyBaseState : IState
    {
        protected readonly FlyingEnemy enemy;
        protected readonly Animator animator;
        
        protected static readonly int FlyHash = Animator.StringToHash("FlyingMove");
        protected static readonly int AttackHash = Animator.StringToHash("FlyingAttack");
        protected static readonly int DieHash = Animator.StringToHash("Die");
        protected static readonly int KnockBackHash = Animator.StringToHash("KnockBack");
        
        protected const float crossFadeDuration = 0.1f;

        protected FlyingEnemyBaseState(FlyingEnemy enemy, Animator animator)
        {
            this.enemy = enemy;
            this.animator = animator;
        }
        
        public virtual void OnEnter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void OnExit() { }
    }
}