using UnityEngine;

namespace Platformer 
{
    public class EnemyWaitState : EnemyBaseState 
    {
        private float changeDirectionTimer;

        public EnemyWaitState(Enemy enemy, Animator animator) : base(enemy, animator) { }

        public override void OnEnter() {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
            enemy.isWaitingForTurn = true; 
            changeDirectionTimer = Random.Range(2f, 5f); 
        }

        public override void Update() {
            enemy.FacePlayer();
            
            // Randomly flip strafe direction to look organic
            changeDirectionTimer -= Time.deltaTime;
            if (changeDirectionTimer <= 0) {
                enemy.circleDirection *= -1; 
                changeDirectionTimer = Random.Range(2f, 5f); 
            }

            enemy.DoStrafe();
            enemy.UpdateAnimation();
        }

        public override void OnExit() {
            enemy.isWaitingForTurn = false;
        }
    }
}