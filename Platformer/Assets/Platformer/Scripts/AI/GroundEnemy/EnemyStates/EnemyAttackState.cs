﻿using UnityEngine;
using UnityEngine.AI;

namespace Platformer {
    public class EnemyAttackState : EnemyBaseState {
        readonly NavMeshAgent agent;
        readonly Transform player;
        
        public EnemyAttackState(Enemy enemy, Animator animator, NavMeshAgent agent, Transform player) : base(enemy, animator) {
            this.agent = agent;
            this.player = player;
        }
        
        public override void OnEnter() {
           // Debug.Log("Attack");
            animator.CrossFade(AttackHash, crossFadeDuration);
        }
        
        public override void Update() {
            agent.SetDestination(player.position);
            // Rotate towards the player
            Vector3 direction = (player.position - enemy.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * enemy.rotationSpeed);

            enemy.Attack();
        }
    }
}