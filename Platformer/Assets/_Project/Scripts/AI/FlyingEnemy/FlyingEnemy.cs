using System;
using UnityEngine;
using KBCore.Refs;
using System.Collections.Generic;
using Utilities;
using Random = UnityEngine.Random;

namespace Platformer 
{ 
    [RequireComponent(typeof(PlayerDetector))] 
    [RequireComponent(typeof(EnemyHealth))]
    public class FlyingEnemy : Entity
    {
        #region Variables
        [field: Header("Component References")]
        [field: SerializeField, Self] PlayerDetector playerDetector;
        [field: SerializeField, Child] Animator animator;
        [field: SerializeField, Self] EnemyHealth enemyHealth;
        
        [field: Header("Movement")]
        [field: SerializeField] float speed = 5f;
        [field: SerializeField] public float rotationSpeed = 5f;
        [field: SerializeField] float stoppingDistance = 1.5f;
        [field: SerializeField] float wanderRadius = 10f; // Replaces Waypoints
        
        [field: Header("Knockback")]
        [SerializeField] float knockbackForce = 10f;
        [SerializeField] float knockbackFriction = 2f;
        Vector3 knockbackVelocity;
        
        [field: Header("Attack")]
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] float damageAmount = 10f;
        
        [field: Header("Timers & StateMachine")]
        List<Timer> timers;
        CountdownTimer attackTimer;
        CountdownTimer knockbackTimer;
        
        StateMachine stateMachine;
        
        // Wander Logic
        Vector3 startPoint;
        Vector3 currentWanderTarget;
        #endregion
        
        void OnValidate() => this.ValidateRefs();


        private void Awake()
        {
            SetupStateMachine();
            SetupTimers();
        }

        void Start()
        {
            startPoint = transform.position;
            currentWanderTarget = GetNewWanderTarget(); // Pick first point
            
            enemyHealth.OnHit += HandleOnHit;
        }
        
        void Update() 
        {
            stateMachine.Update();
            HandleTimers();
          
        }
        
        void FixedUpdate() => stateMachine.FixedUpdate();

        void OnDestroy()
        {
            enemyHealth.OnHit -= HandleOnHit;
        }
        
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
            
            attackTimer.Tick(Time.deltaTime);
            knockbackTimer.Tick(Time.deltaTime);
            
            timers = new List<Timer>(2)
                { attackTimer, knockbackTimer };
        }

        #endregion


        #region StateMachine
        private void SetupStateMachine()
        {
            stateMachine = new StateMachine();

            // Declare States (Clean constructors)
            var wanderState = new FlyingEnemyWanderState(this, animator);
            var chaseState = new FlyingEnemyChaseState(this, animator);
            var attackState = new FlyingEnemyAttackState(this, animator);
            var dieState = new FlyingEnemyDieState(this, animator, enemyHealth);
            var knockbackState = new FlyingEnemyKnockbackState(this, animator);

            // Transitions
            At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
            At(chaseState, wanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
            At(chaseState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer()));
            At(attackState, chaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer()));
            
            // Knockback Transitions
            Any(knockbackState, new FuncPredicate(() => knockbackTimer.IsRunning));
            At(knockbackState, chaseState, new FuncPredicate(() => !knockbackTimer.IsRunning));
            
            Any(dieState, new FuncPredicate(() => enemyHealth.isDead));

            stateMachine.SetState(wanderState);
        }
        
        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
        #endregion
        
        #region Wander Behaviour

        public void HandleWander()
        {
            // Move towards the random point
            RotateTowards(currentWanderTarget);
            transform.position = Vector3.MoveTowards(transform.position, currentWanderTarget, speed * Time.deltaTime);

            // If we reached the point, pick a new one
            if (Vector3.Distance(transform.position, currentWanderTarget) < 0.5f)
            {
                currentWanderTarget = GetNewWanderTarget();
            }
        }

        Vector3 GetNewWanderTarget()
        {
            // Pick a random point inside a 3D sphere around the start point
            return startPoint + Random.insideUnitSphere * wanderRadius;
        }
        
        void RotateTowards(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            if(direction == Vector3.zero) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        public void HandleChase()
        {
            if (playerDetector.Player == null) return;

            RotateTowards(playerDetector.Player.position);
            
            if (Vector3.Distance(transform.position, playerDetector.Player.position) > stoppingDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerDetector.Player.position, speed * Time.deltaTime);
            }
        }
        #endregion
        
        #region KnockBack
        void HandleOnHit(float duration)
        {
            knockbackTimer.Reset(duration);
            knockbackTimer.Start();

            if (playerDetector.Player != null)
            {
                // Push AWAY from player
                Vector3 direction = (transform.position - playerDetector.Player.position).normalized;
                knockbackVelocity = direction * knockbackForce;
            }
        }

        public void HandleKnockback()
        {
            transform.position += knockbackVelocity * Time.deltaTime;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * knockbackFriction);
        }
        #endregion

        #region Attack
        public void HandleAttack()
        {
            if (playerDetector.Player == null) return;
            
            RotateTowards(playerDetector.Player.position);
            Attack();
        }
        
        public void Attack()
        {
            if (attackTimer.IsRunning) return;
            attackTimer.Start();

            if (playerDetector.PlayerHealth != null && !playerDetector.PlayerHealth.IsInvulnerable)
            {
                playerDetector.PlayerHealth.TakeDamage(damageAmount);
            }
            
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyAttack, this.transform.position);
        }
        #endregion

       
    }
}