using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace Platformer {
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
    [RequireComponent(typeof(Health))]
    public class Enemy : Entity 
    {
        #region Variables
        [field: Header("Component References")]
        [field: SerializeField, Self] public NavMeshAgent Agent { get; private set; }
        [field: SerializeField, Self] PlayerDetector playerDetector;
        [field: SerializeField, Child] Animator animator;
        [field: SerializeField, Self] Health enemyHealth;
        
        [field: Header("Movement Settings")]
        [SerializeField] float wanderRadius = 10f;
        
        [field: Header("Knockback Settings")]
        [SerializeField] float knockbackForce = 8f;
        [SerializeField] float knockbackFriction = 5f;
        
        [field: Header("Attack")]
        [field: SerializeField] float timeBetweenAttacks = 1f;
        [field: SerializeField] float damageAmount = 10f;
        
        [field: Header("Timers & StateMachine")]
        List<Timer> timers;
        CountdownTimer attackTimer;
        CountdownTimer knockbackTimer;
        
        StateMachine stateMachine;
        
        // Stored for Wander logic
        Vector3 startPoint;
        Vector3 knockbackVelocity;
        #endregion

        void OnValidate() => this.ValidateRefs();

        private void Awake()
        {
            SetupStateMachine();
            SetupTimers();
        }
        void Start()
        {
            enemyHealth.OnHit += HandleOnHit;
            startPoint = transform.position;
        }
        
        void OnDestroy()
        {
            enemyHealth.OnHit -= HandleOnHit;
        }

        void Update() 
        {
            stateMachine.Update();
            attackTimer.Tick(Time.deltaTime);
            HandleTimers();
        }
        void FixedUpdate() => stateMachine.FixedUpdate();

        #region StateMachine
        private void SetupStateMachine()
        {
            
            stateMachine = new StateMachine();
            
            // States now just pass 'this'
            var wanderState = new EnemyWanderState(this, animator);
            var chaseState = new EnemyChaseState(this, animator);
            var attackState = new EnemyAttackState(this, animator);
            var dieState = new EnemyDieState(this, animator, enemyHealth);
            var knockbackState = new EnemyKnockbackState(this, animator);
            
            // Transitions
            At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
            At(chaseState, wanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
            At(chaseState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer()));
            At(attackState, chaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer()));
            At(knockbackState, chaseState, new FuncPredicate(() => !knockbackTimer.IsRunning));
            
            Any(knockbackState, new FuncPredicate(() => knockbackTimer.IsRunning));
            Any(dieState, new FuncPredicate(() => enemyHealth.isDead));
            
            stateMachine.SetState(wanderState);
        }
        
        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
        #endregion

        #region Timers
        void HandleTimers()
        {
            foreach (var timer in timers)
            {
                timer.Tick(Time.deltaTime);
            }
        }

        void SetupTimers()
        {
            attackTimer = new CountdownTimer(timeBetweenAttacks);
            knockbackTimer = new CountdownTimer(0.5f);
            
            timers = new List<Timer>(2)
                { attackTimer, knockbackTimer };
        }

        #endregion

        void HandleOnHit(float stunDuration)
        {
            // Reset and Start the timer with the specific stun duration from the attack
            knockbackTimer.Reset(stunDuration);
            knockbackTimer.Start();

            // Calculate Direction: Away from Player
            // (If you want perfect accuracy, pass the attacker's position, but this works for 99% of cases)
            if (playerDetector.Player != null)
            {
                Vector3 pushDir = (transform.position - playerDetector.Player.position).normalized;
                pushDir.y = 0; // Don't fly upwards
                knockbackVelocity = pushDir * knockbackForce;
            }
        }

        // 4. The Physics Logic calling in the State
        public void HandleKnockbackPhysics()
        {
            // Apply the velocity to the Agent
            Agent.velocity = knockbackVelocity;

            // Apply "Friction" to slow down smoothly
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * knockbackFriction);
        }

        #region Enemy Movement Behavior
        public void HandleWander()
        {
            if (HasReachedDestination()) 
            {
                var randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += startPoint;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
                var finalPosition = hit.position;
                
                Agent.SetDestination(finalPosition);
            }
        }
        
        public void HandleChase()
        {
            Agent.SetDestination(playerDetector.Player.position);
        }

        bool HasReachedDestination() 
        {
            return !Agent.pathPending
                   && Agent.remainingDistance <= Agent.stoppingDistance
                   && (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f);
        }
        #endregion

        #region Attack
        public void HandleAttack()
        {
            if (playerDetector.Player != null)
            {
                Agent.SetDestination(playerDetector.Player.position);

                // Handle Rotation
                Vector3 direction = (playerDetector.Player.position - transform.position).normalized;
                // Zero out Y to prevent tilting
                direction.y = 0; 
                
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
            }
            Attack();
        }

        public void Attack() 
        {
            if (attackTimer.IsRunning) return;
            attackTimer.Start();
            
            // FIXED: Only applying damage once
            if (playerDetector.PlayerHealth != null && !playerDetector.PlayerHealth.IsInvulnerable) 
            { 
                playerDetector.PlayerHealth.TakeDamage(damageAmount);
            }
        }

        public void PlayAttackSfx()
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyAttack, this.transform.position);
        }
        #endregion
    }
}