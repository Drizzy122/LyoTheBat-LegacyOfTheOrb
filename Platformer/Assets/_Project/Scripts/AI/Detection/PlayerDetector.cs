using UnityEngine;
using ImprovedTimers;

namespace Platformer {
    public class PlayerDetector : MonoBehaviour {
        [field: Header("Detection Gizmos")]
        [SerializeField] float detectionAngle = 60f; // Cone in front of enemy
        [SerializeField] [Range(0,20)] float detectionRadius = 10f; // Large circle around enemy
        [SerializeField] [Range(0,20)] float innerDetectionRadius = 5f; // Small circle around enemy
        
        [field: Header("Detection Strategies")]
        [SerializeField] float detectionCooldown = 1f; // Time between detections
        [SerializeField] public float attackRange = 2f; // Distance from enemy to player to attack
        
        public Transform Player { get; private set; }
        public Health PlayerHealth { get; set; }
        
        CountdownTimer detectionTimer;
        IDetectionStrategy detectionStrategy;

        void Awake() {
            detectionTimer = new CountdownTimer(detectionCooldown);
            detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);
        }

        void Start() {
            Player = GameObject.FindGameObjectWithTag("Player").transform; // Make sure to TAG the player!
            PlayerHealth = Player.GetComponent<Health>() ?? Player.GetComponentInParent<Health>();
        }
        
        void Update()
        {
            detectionTimer.Tick();
        }

        public bool CanDetectPlayer() {
            return detectionTimer.IsRunning || detectionStrategy.Execute(Player, transform, detectionTimer);
        }

        public bool CanAttackPlayer() {
            var directionToPlayer = Player.position - transform.position;
            return directionToPlayer.magnitude <= attackRange;
        }
        
        public void SetDetectionStrategy(IDetectionStrategy detectionStrategy) => this.detectionStrategy = detectionStrategy;
        
        void OnDrawGizmos() {
            Gizmos.color = Color.black;

            // Draw a spheres for the radii
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.DrawWireSphere(transform.position, innerDetectionRadius);
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Calculate our cone directions
            Vector3 forwardConeDirection = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward * detectionRadius;
            Vector3 backwardConeDirection = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward * detectionRadius;

            // Draw lines to represent the cone
            Gizmos.DrawLine(transform.position, transform.position + forwardConeDirection);
            Gizmos.DrawLine(transform.position, transform.position + backwardConeDirection);
        }
    }
}