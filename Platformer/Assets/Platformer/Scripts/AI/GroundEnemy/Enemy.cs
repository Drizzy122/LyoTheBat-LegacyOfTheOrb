﻿using KBCore.Refs;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace Platformer {
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
    [RequireComponent(typeof(EnemyHealth))]
    public class Enemy : Entity {
        [SerializeField, Self] NavMeshAgent agent;
        [SerializeField, Self] PlayerDetector playerDetector;
        [SerializeField, Child] Animator animator;
        
        [SerializeField] float wanderRadius = 10f;
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] float damageAmount = 10f;
        [SerializeField] public float rotationSpeed = 5f; // Adjust this in the Inspector
        
        [SerializeField] public float RunSpeed = 6f; // You can adjust this in the Inspector
        
        public float knockbackTimer;
        public float knockbackSpeed = 2f;
        public Vector3 knockBackDirection;
        public float turnSpeedToReturnTo;
        public float accelerationToReturnTo;

        StateMachine stateMachine;
        private EnemyHealth enemyHealth;
        CountdownTimer attackTimer;
        [SerializeField] private Transform[] waypoints; // Assign in the Inspector

        void OnValidate() => this.ValidateRefs();

        void Start() {
            attackTimer = new CountdownTimer(timeBetweenAttacks);
            stateMachine = new StateMachine();
            enemyHealth = GetComponent<EnemyHealth>();
            
            var wanderState = new EnemyWanderState(this, animator, agent, wanderRadius, waypoints);
            var chaseState = new EnemyChaseState(this, animator, agent, playerDetector.Player);
            var attackState = new EnemyAttackState(this, animator, agent, playerDetector.Player);
            var dieState = new  EnemyDieState(this, animator);
            var knockbackState = new EnemyKnockbackState(this, animator, agent, playerDetector.Player);
            
            At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
            At(chaseState, wanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
            At(chaseState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer()));
            At(attackState, chaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer()));
            At(knockbackState, chaseState, new FuncPredicate(() => knockbackTimer <= 0));
            
            Any(knockbackState, new FuncPredicate(() => enemyHealth.currentHealth > 0 && knockbackTimer > 0));
            Any(dieState, new FuncPredicate(() => enemyHealth.currentHealth <= 0));
            
            stateMachine.SetState(wanderState);
            enemyHealth.OnDeath += () => stateMachine.SetState(dieState);

            accelerationToReturnTo = agent.acceleration;
            turnSpeedToReturnTo = agent.angularSpeed;
        }
        
        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        void Update() {
            stateMachine.Update();
            attackTimer.Tick(Time.deltaTime);
        }
        
        void FixedUpdate() {
            stateMachine.FixedUpdate();

            if (knockbackTimer > 0)
            {
                knockbackTimer -= Time.fixedDeltaTime;
            }
        }
        
        public void Attack() {
            if (attackTimer.IsRunning) return;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyAttack, this.transform.position);
            attackTimer.Start();

            // Access PlayerHealth and apply damage only if the player is not invulnerable
            var playerHealth = playerDetector.Player.GetComponent<PlayerHealth>();
            if (playerHealth != null) 
            {
                if (!playerHealth.IsInvulnerable) 
                { 
                    playerHealth.TakeDamage(damageAmount);
                } 
            } 
        }

        private void OnDrawGizmosSelected()
        {
            if (knockBackDirection != Vector3.zero)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + knockBackDirection);
            }
        }
    }
}