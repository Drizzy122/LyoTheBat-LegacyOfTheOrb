using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using Utilities;
using DG.Tweening;

namespace Platformer 
{
    [RequireComponent(typeof(CharacterController))]
    public class Enemy : Entity 
    {
        #region Variables
        [field: Header("Component References")]
        [field: SerializeField, Self] public CharacterController Controller { get; private set; }
        [field: SerializeField, Self] public PlayerDetector playerDetector;
        [field: SerializeField, Child] public Animator animator;
        [field: SerializeField, Self] public Health enemyHealth;
        
        [field: Header("Movement Settings")]
        [field: SerializeField] public float wanderSpeed = 2f;
        [field: SerializeField] public float chaseSpeed = 5f;
        [field: SerializeField] public float strafeSpeed = 3f;
        [field: SerializeField] public float circleRadius = 5f;
        [field: SerializeField] float wanderRadius = 10f;
        public int circleDirection = 1; 
        
        [field: Header("Knockback Settings")]
        [SerializeField] float knockbackForce = 8f;
        [SerializeField] float knockbackFriction = 5f;
        
        [field: Header("Attack")]
        [field: SerializeField] float timeBetweenAttacks = 1f;
        [field: SerializeField] float damage = 10f;
        
        public bool hasAttackOrders = false;
        public bool isWaitingForTurn = false;
        [HideInInspector] public bool isPreparingAttack = false;
        
        [field: Header("Polish")]
        [SerializeField] private ParticleSystem counterParticle;
        
        private Tween attackWarningTween; 
        private Tween attackMoveTween;    
        
        [field: Header("Timers & StateMachine")]
        List<Timer> timers;
        CountdownTimer attackTimer;
        public CountdownTimer knockbackTimer;
        StateMachine stateMachine;
        
        Vector3 startPoint;
        Vector3 currentVelocity;
        Vector3 knockbackVelocity;
        Vector3 wanderTarget;
        float wanderTimer;
        
        private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
        private static readonly int VelocityYHash = Animator.StringToHash("VelocityY");
        #endregion

        void OnValidate() => this.ValidateRefs();

        private void Awake()
        {
            startPoint = transform.position;
            Controller = GetComponent<CharacterController>();
            SetupTimers();
            SetupStateMachine();
        }
        
        private void Start()
        {
            // Pick a random direction to strafe when spawned
            circleDirection = Random.value > 0.5f ? 1 : -1;
        }

        #region StateMachine & Timers
        private void SetupStateMachine() 
        {
            stateMachine = new StateMachine();

            var wanderState = new EnemyWanderState(this, animator);
            var chaseState = new EnemyChaseState(this, animator);
            var waitState = new EnemyWaitState(this, animator); 
            var attackState = new EnemyAttackState(this, animator);
            var dieState = new EnemyDieState(this, animator, enemyHealth);
            var knockbackState = new EnemyKnockbackState(this, animator);

            // Transitions
            At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
            
            At(chaseState, waitState, new FuncPredicate(() => Vector3.Distance(transform.position, playerDetector.Player.position) <= circleRadius));
            
            At(waitState, chaseState, new FuncPredicate(() => Vector3.Distance(transform.position, playerDetector.Player.position) > circleRadius + 1f));
            
            At(waitState, attackState, new FuncPredicate(() => hasAttackOrders));
            At(attackState, waitState, new FuncPredicate(() => !hasAttackOrders));

            Any(knockbackState, new FuncPredicate(() => knockbackTimer.IsRunning));
            At(knockbackState, chaseState, new FuncPredicate(() => !knockbackTimer.IsRunning));
            
            Any(dieState, new FuncPredicate(() => enemyHealth.isDead));

            stateMachine.SetState(wanderState);
        }

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        void SetupTimers()
        {
            attackTimer = new CountdownTimer(timeBetweenAttacks);
            knockbackTimer = new CountdownTimer(0.5f);
            timers = new List<Timer>(2) { attackTimer, knockbackTimer };
        }

        void HandleTimers()
        {
            foreach (var timer in timers) timer.Tick(Time.deltaTime);
        }
        #endregion

        #region Unity Updates
        private void Update() 
        {
            stateMachine.Update();
            HandleTimers();
            ApplyGravity();
        }

        private void FixedUpdate() => stateMachine.FixedUpdate();

        private void ApplyGravity() 
        {
            if (!Controller.enabled) return; // Skip gravity if DOTween is actively lunging!
            
            if (!Controller.isGrounded) {
                currentVelocity.y += Physics.gravity.y * Time.deltaTime;
            } else {
                currentVelocity.y = -2f; // Stick firmly to the ground
            }
            Controller.Move(currentVelocity * Time.deltaTime);
        }

        public void FacePlayer() 
        {
            if (playerDetector.Player != null) {
                Vector3 dir = (playerDetector.Player.position - transform.position).normalized;
                dir.y = 0;
                if (dir != Vector3.zero) {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
                }
            }
        }

        public void UpdateAnimation()
        {
            // Converts world velocity to local velocity for the Animator Blend Tree!
            Vector3 relativeVelocity = transform.InverseTransformDirection(Controller.velocity);
            animator.SetFloat(VelocityXHash, relativeVelocity.x, 0.1f, Time.deltaTime);
            animator.SetFloat(VelocityYHash, relativeVelocity.z, 0.1f, Time.deltaTime);
        }
        #endregion

        #region Movement Methods
        public void DoWander() 
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0 || Vector3.Distance(transform.position, wanderTarget) < 1f) {
                wanderTarget = startPoint + (Random.insideUnitSphere * wanderRadius);
                wanderTarget.y = transform.position.y;
                wanderTimer = Random.Range(2f, 5f);
            }
            MoveTowards(wanderTarget, wanderSpeed);
        }

        public void DoChase() 
        {
            if (playerDetector.Player != null) {
                MoveTowards(playerDetector.Player.position, chaseSpeed);
            }
        }

        public void DoStrafe() 
        {
            if (playerDetector.Player == null) return;

            Vector3 dirToPlayer = (playerDetector.Player.position - transform.position).normalized;
            dirToPlayer.y = 0;
            
            // The Mix & Jam 90-degree sideways vector math
            Vector3 strafeDir = Quaternion.AngleAxis(90, Vector3.up) * dirToPlayer;
            Vector3 moveVec = strafeDir * circleDirection * strafeSpeed;
            
            // Gentle correction to keep them perfectly on the ring
            float dist = Vector3.Distance(transform.position, playerDetector.Player.position);
            if (dist > circleRadius + 0.2f) moveVec += dirToPlayer * strafeSpeed;
            else if (dist < circleRadius - 0.2f) moveVec -= dirToPlayer * strafeSpeed;

            Controller.Move(moveVec * Time.deltaTime);
        }

        private void MoveTowards(Vector3 target, float speed) 
        {
            Vector3 dir = (target - transform.position).normalized;
            dir.y = 0;
            Controller.Move(dir * speed * Time.deltaTime);
            
            if (dir != Vector3.zero) {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
            }
        }
        #endregion

        #region Combat & Polish
        public void GiveAttackOrder() => hasAttackOrders = true;
        public void FinishAttack() => hasAttackOrders = false;

        public void Attack() 
        {
            if (attackTimer.IsRunning) return;
            attackTimer.Start();
    
            if (playerDetector.PlayerHealth != null && !playerDetector.PlayerHealth.IsInvulnerable) 
            { 
                playerDetector.PlayerHealth.TakeDamage(damage);
            }
        }

        public void LungeAtPlayer()
        {
            if (playerDetector.Player != null)
            {
                isPreparingAttack = true; 
                Vector3 direction = (playerDetector.Player.position - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);

                ShowCounterWarning(true);
                Controller.enabled = false; // Turn off physics for DOTween!

                attackWarningTween = DOVirtual.DelayedCall(0.4f, () => 
                {
                    ShowCounterWarning(false); 
                    isPreparingAttack = false; 

                    Vector3 stopPosition = playerDetector.Player.position - (direction * 1.5f);

                    // 1. LUNGE
                    attackMoveTween = transform.DOMove(stopPosition, 0.3f).SetEase(Ease.OutQuad).OnComplete(() => 
                    {
                        Attack(); // Deal Damage
                        
                        // 2. RETREAT
                        Vector3 retreatPosition = transform.position - (transform.forward * 4f);
                        transform.DOMove(retreatPosition, 0.4f).SetEase(Ease.OutCubic).SetDelay(0.2f).OnComplete(() =>
                        {
                            Controller.enabled = true; // Turn physics back on!
                            FinishAttack();       
                        });
                    });
                });
            }
        }
        
        public void CancelAttack()
        {
            isPreparingAttack = false;
            ShowCounterWarning(false);
            attackWarningTween?.Kill();
            attackMoveTween?.Kill();
            Controller.enabled = true;
            FinishAttack(); 
        }
        
        void HandleOnHit(float stunDuration)
        {
            knockbackTimer.Reset(stunDuration);
            knockbackTimer.Start();

            if (playerDetector.Player != null)
            {
                Vector3 pushDir = (transform.position - playerDetector.Player.position).normalized;
                pushDir.y = 0; 
                knockbackVelocity = pushDir * knockbackForce;
            }
        }

        public void HandleKnockbackPhysics()
        {
            if (Controller.enabled)
            {
                Controller.Move(knockbackVelocity * Time.deltaTime);
                knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * knockbackFriction);
            }
        }
        
        public void ShowCounterWarning(bool active)
        {
            if (counterParticle == null) return;
            if (active) counterParticle.Play();
            else { counterParticle.Clear(); counterParticle.Stop(); }
        }

        private void OnEnable() => enemyHealth.OnHit += HandleOnHit;
        private void OnDisable() => enemyHealth.OnHit -= HandleOnHit;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, circleRadius);
        }
        #endregion
    }
}