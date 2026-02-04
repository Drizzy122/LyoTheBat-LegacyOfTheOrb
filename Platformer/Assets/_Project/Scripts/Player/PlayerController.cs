using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using Utilities;
using UnityEngine.Rendering.Universal;
using FMOD.Studio;
using Platformer;
using UnityEngine.UI;

namespace Platformer
{
    public class PlayerController : ValidatedMonoBehaviour , IDataPersistence
    {
        #region Variables
        [field: Header("References")] 
        [field: SerializeField, Anywhere] InputReader input;
        [field: SerializeField, Self] Rigidbody rb;
        [field: SerializeField, Self] GroundChecker groundChecker;
        [field: SerializeField, Self] Animator animator;
        [field: SerializeField, Self] private FootstepController footstepController;
        [field: SerializeField, Self] Health playerHealth;
        [field: SerializeField, Self] GlideStamina glideStamina;
        
        [Header("Movement Settings")]
        [field: SerializeField] float moveSpeed = 6f;
        [field: SerializeField] float rotationSpeed = 15f;
        [field: SerializeField] float smoothTime = 0.2f;
        
        [field: Header("Jump Settings")] 
        [field: SerializeField] float jumpForce = 10f;
        [field: SerializeField] float jumpDuration = 0.5f;
        [field: SerializeField] float jumpCooldown = 0f;
        [field: SerializeField] float gravityMultiplier = 3f;
        [field: SerializeField] private int jumpCount = 2;
        [field: SerializeField] private int remainingJumps = 2;
        [field: SerializeField] private int maxFallSpeed = 10;

        [field: Header("Dash Settings")] 
        [field: SerializeField] float dashForce = 10f;
        [field: SerializeField] float dashDuration = 1f;
        [field: SerializeField] float dashCooldown = 2f;
        
        
        
        
        
        [field: Header("Echolocation Settings")] 
        [field: SerializeField] float echoCooldown = 0.5f;
        [field: SerializeField] float detectionRadius = 5f; 
        public LayerMask detectionLayer;
        [field: SerializeField] ParticleSystem detectionParticle;
        public ScriptableRendererFeature echoRendererFeature;
        private ScriptableRendererData rendererData;
        [field: SerializeField] private float echoDuration = 5f;
        [field: SerializeField] private int maxEchoCharges = 3;
        [field: SerializeField] private float chargeRegenerationTime = 15f; 
        private int currentEchoCharges;
        [field: SerializeField] Image echoChargeUI;
        private bool isRegenerating = false;
        
        [field: Header("Glide Settings")] 
        [field: SerializeField] public float glideBoost = 1;
        [field: SerializeField] float glideBoostDecayRate = 0.02f;
        [field: SerializeField] float glideFallSpeed = 0.1f;
        [field: SerializeField] float glideMoveSpeed = 2;
        [field: SerializeField] public float glideTime = 3;
        [field: SerializeField] GameObject defaultMesh;  
        [field: SerializeField] GameObject glidingMesh;

        [field: Header("Attack Settings")]
        [field: SerializeField] float attackCoolDown = 0.5f;
        [field: SerializeField] float attackDistance = 1f;
        [field: SerializeField] float spinAttackDistance = 5f;
        [field: SerializeField] int attackDamage = 10;
        [field: SerializeField] int spinAttackDamage = 20;
        [field: SerializeField] float knockbackTime = 0.5f;
        
        [Header("Interact")] 
        [field: SerializeField] float interactDistance = 5;
        
        [field: Header("Wall Climb Settings")] 
        [field: SerializeField] float wallCheckDist = 1f;
        [field: SerializeField] float wallClimbMoveSpeed = 5f;
        [field: SerializeField] LayerMask wallClimbLayer;

        [field: Header("More variables")]
        public bool wallClimbimg;
        bool[] wallClimbChecks;
        Vector3 wallClimbNormal;
        Vector3 wallClimbTargetPos;
        public Vector3 wallClimbPos;
        const float ZeroF = 0f;
       
        float currentSpeed;
        float velocity;
        float jumpVelocity;
        float dashVelocity = 1f;
        public bool isTeleporting;
        Vector3 movement;
        Transform mainCam;
        
        [field: Header("Timers")] 
        List<Timer> timers;
        CountdownTimer jumpTimer;
        CountdownTimer jumpCooldownTimer;
        CountdownTimer dashTimer;
        CountdownTimer dashCooldownTimer;
        CountdownTimer attackTimer;
        CountdownTimer spinAttackTimer;
        CountdownTimer echoTimer;
        CountdownTimer glideTimer;

        StateMachine stateMachine;
        private EventInstance playerFootsteps;
        static readonly int Speed = Animator.StringToHash("Speed");
        #endregion
        void Awake()
        {
            mainCam = Camera.main.transform;
            rb.freezeRotation = true;
            glideStamina = GetComponent<GlideStamina>();
            SetupTimers();
            SetupStateMachine();
        }
        void Start()
        {
            playerFootsteps = AudioManager.instance.CreateEventInstance(FMODEvents.instance.playerFootsteps);
            input.EnablePlayerActions();
            currentEchoCharges = maxEchoCharges;
            UpdateEchoChargeUI();
        }
        void Update()
        {
            movement = new Vector3(input.Direction.x, 0f, input.Direction.y);
            stateMachine.Update();
            
            HandleTimers();
            TriggerFootstepEvents();
            UpdateAnimator();
        }
        void FixedUpdate()
        {
            stateMachine.FixedUpdate();
            WallClimbCheck();
        }
        void UpdateAnimator() => animator.SetFloat(Speed, currentSpeed);
        void UpdateSound()
        {
            if (rb.linearVelocity.x != 0 && groundChecker.IsGrounded)
            {
                PLAYBACK_STATE playbackState;
                playerFootsteps.getPlaybackState(out playbackState);

                if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    playerFootsteps.start();
                }

                // Adjust the sound properties based on movement speed
                playerFootsteps.setParameterByName("moveSpeed", moveSpeed);
            }
            else
            {
                playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }

        #region StateMachine
        private void SetupStateMachine()
        {
            // State Machine
            stateMachine = new StateMachine();

            // Declare states
            var locomotionState = new LocomotionState(this, animator);
            var jumpState = new JumpState(this, animator);
            var doubleJumpState = new DoubleJumpState(this, animator);
            var glideState = new GlideState(this, animator);
            var dashState = new DashState(this, animator);
            var attackState = new AttackState(this, animator);
            var spinAttackState = new SpinAttackState(this, animator);
            var deathState = new DeathState(this, animator, playerHealth);
            var echoLocationState = new EcholocationState(this, animator);
            var teleportState = new TeleportState(this, animator);
            var wallClimbState = new WallClimbState(this, animator);
            var swimState = new SwimState(this, animator);
            
            // Define transitions for jump 
            At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
            At(locomotionState, jumpState, new FuncPredicate(() => !groundChecker.IsGrounded));

            // Define transitions for double jump
            At(doubleJumpState, glideState, new FuncPredicate(() => glideTimer.IsRunning));
            At(jumpState, doubleJumpState, new FuncPredicate(() => jumpTimer.IsRunning && remainingJumps <= jumpCount - 2));
            At(doubleJumpState, dashState, new FuncPredicate(() => dashTimer.IsRunning));

            // Define transitions for Dash 
            At(locomotionState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(glideState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(jumpState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(dashState, jumpState, new FuncPredicate(() => !dashTimer.IsRunning));
            
            // Define transitions for attack
            At(locomotionState, attackState, new FuncPredicate(() => attackTimer.IsRunning));
            At(attackState, locomotionState, new FuncPredicate(() => !attackTimer.IsRunning));
            
            At(locomotionState, spinAttackState, new FuncPredicate(() => spinAttackTimer.IsRunning));
            At(spinAttackState, locomotionState, new FuncPredicate(() => !spinAttackTimer.IsRunning));
            
            // Enable Spin Attack while jumping
            At(jumpState, spinAttackState, new FuncPredicate(() => spinAttackTimer.IsRunning));
            At(spinAttackState, jumpState, new FuncPredicate(() => !spinAttackTimer.IsRunning || jumpTimer.IsRunning));

            // Define transitions to echo state
            At(locomotionState, echoLocationState, new FuncPredicate(() => echoTimer.IsRunning));
            At(echoLocationState, locomotionState, new FuncPredicate(() => !echoTimer.IsRunning));

            // Define transitions for glide
            At(jumpState, glideState, new FuncPredicate(() => glideTimer.IsRunning));
            At(dashState, glideState, new FuncPredicate(() => glideTimer.IsRunning));
            At(glideState, jumpState, new FuncPredicate(() => !glideTimer.IsRunning));
            
            // Define transitions for wall climb
            At(jumpState, wallClimbState, new FuncPredicate(() => wallClimbimg));
            At(doubleJumpState, wallClimbState, new FuncPredicate(() => wallClimbimg));
            At(wallClimbState, doubleJumpState, new FuncPredicate(() => !wallClimbimg));
            
            // Definne transition for teleportation
             At(teleportState, locomotionState, new FuncPredicate(() => !isTeleporting));
            
             At(locomotionState, swimState, new FuncPredicate(() => InWater));
             At(jumpState, swimState, new FuncPredicate(() => InWater));
             At(doubleJumpState, swimState, new FuncPredicate(() => InWater));
             At(glideState, swimState, new FuncPredicate(() => InWater));

// 2. Transition FROM Swim State
// If we exit the water trigger, go back to Locomotion (or Jump if you implement jumping out of water)
             At(swimState, locomotionState, new FuncPredicate(() => !InWater));
             
            // Set initial state
            Any(teleportState, new FuncPredicate(() => isTeleporting));
            Any(deathState, new FuncPredicate(() => playerHealth.isDead));
            Any(locomotionState, new FuncPredicate(ReturnToLocomotionState));
            
            stateMachine.SetState(locomotionState);
        }

        bool ReturnToLocomotionState()
        {
            return groundChecker.IsGrounded
                   && !InWater 
                   && !playerHealth.isDead
                   && !attackTimer.IsRunning
                   && !spinAttackTimer.IsRunning
                   && !jumpTimer.IsRunning
                   && !dashTimer.IsRunning
                   && !glideTimer.IsRunning
                   && !echoTimer.IsRunning
                   && !isTeleporting;

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
            // Setup timers
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);

            jumpTimer.OnTimerStart += () => jumpVelocity = jumpForce;
            jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();

            dashTimer = new CountdownTimer(dashDuration);
            dashCooldownTimer = new CountdownTimer(dashCooldown);

            dashTimer.OnTimerStart += () => dashVelocity = dashForce;
            dashTimer.OnTimerStop += () =>
            {
                dashVelocity = 1f;
                dashCooldownTimer.Start();
            };

            echoTimer = new CountdownTimer(echoCooldown);
            glideTimer = new CountdownTimer(glideTime);
            attackTimer = new CountdownTimer(attackCoolDown);
            spinAttackTimer = new CountdownTimer(attackCoolDown);

            timers = new List<Timer>(8)
                { jumpTimer, jumpCooldownTimer, dashTimer, dashCooldownTimer, attackTimer,spinAttackTimer, echoTimer, glideTimer };
        }
        #endregion

        #region Movement
        public void HandleMovement()
        {
            // Rotate movement direction to match camera rotation
            var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;

            if (adjustedDirection.magnitude > ZeroF)
            {
                HandleRotation(adjustedDirection);
                if (glideTimer.IsRunning) adjustedDirection *= glideMoveSpeed;
                HandleHorizontalMovement(adjustedDirection * glideBoost);
                SmoothSpeed(adjustedDirection.magnitude);
            }
            else
            {
                SmoothSpeed(ZeroF);

                // Reset horizontal velocity for a snappy stop
                rb.linearVelocity = new Vector3(ZeroF, rb.linearVelocity.y, ZeroF);
            }
            UpdateSound();
        }
        void HandleHorizontalMovement(Vector3 adjustedDirection)
        {
            // Move player
            Vector3 velocity = adjustedDirection * (moveSpeed * dashVelocity * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }
        void HandleRotation(Vector3 adjustedDirection)
        {
            // Adjust rotation to match movement Direction
            var targetRotation = Quaternion.LookRotation(adjustedDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
           // transform.LookAt(transform.position + adjustedDirection);
        }
        void SmoothSpeed(float value)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
        }
        void TriggerFootstepEvents()
        {
            if (groundChecker.IsGrounded && currentSpeed > 0.1f)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
                {
                    // Trigger left foot during certain frames
                    if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 < 0.5f)
                        footstepController.LeftFootDown();
                    else
                        footstepController.RightFootDown();
                }
            }
        }
        #endregion

        #region WallClimb
        void OnWallClimb(bool performed)
        {
            if (performed)
            {
                if (wallClimbimg)
                {
                    wallClimbimg = false;
                    if (remainingJumps > 0)
                    {
                        OnJump(true);
                    }
                    else
                    {
                        remainingJumps++;
                        OnJump(true);
                    }
                }
                else
                {
                    if (wallClimbChecks[5])
                    {
                        wallClimbimg = true;
                    }
                }
            }
        }
        public void HandleWallClimb()
        {
            //movement = new Vector3(input.Direction.x, 0f, input.Direction.y);

            rb.linearVelocity = Vector3.zero;
            transform.position = wallClimbPos;

            transform.LookAt(transform.position - wallClimbNormal);
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            if (movement.magnitude > 0)
            {
                if (movement.z > 0)
                {
                    wallClimbPos += transform.up * 0.01f * wallClimbMoveSpeed;
                }
                else if (movement.z < 0)
                {
                    wallClimbPos -= transform.up * 0.01f * wallClimbMoveSpeed;
                }
         
                if (movement.x > 0)
                {
                    wallClimbPos += transform.right * 0.01f * wallClimbMoveSpeed;
                    wallClimbPos += transform.right * 0.01f * wallClimbMoveSpeed;
                }
                else if (movement.x < 0)
                {
                    wallClimbPos -= transform.right * 0.01f * wallClimbMoveSpeed;
                }

                if (!wallClimbChecks[5] && ! wallClimbChecks[2])
                {
                    wallClimbimg = false;
                }
            }

            currentSpeed = movement.magnitude;
        }
        /// <summary>
        /// Wallclimb Checks go in this order:
        /// 0 = spherecast in front to see if wallclimbing can be triggered
        /// 1 = above
        /// 2 = below
        /// 3 = right
        /// 4 = left
        /// 5 = raycast to find normal of wall found in check #0
        /// </summary>
        void WallClimbCheck()
        {
            wallClimbChecks = new bool[6];
            for (int i = 0; i < wallClimbChecks.Length; i++)
            {
                wallClimbChecks[i] = false;
            }

            wallClimbChecks[0] = (Physics.SphereCastAll(
                transform.position + transform.forward * wallCheckDist + transform.up, 
                0.5f,
                transform.forward,
                0.1f,
                wallClimbLayer).Length > 0);

            wallClimbChecks[1] = Physics.Raycast(transform.position + transform.up * 2, transform.forward, 1, wallClimbLayer);
            wallClimbChecks[2] = Physics.Raycast(transform.position - transform.up * 0.6f, transform.forward, 1, wallClimbLayer);
    
            //wallClimbChecks[3] = Physics.Raycast(transform.position + transform.right * 1 + transform.up * 0.5f, transform.forward, 1, wallClimbLayer);
            //wallClimbChecks[4] = Physics.Raycast(transform.position - transform.right * 1 + transform.up * 0.5f, transform.forward, 1, wallClimbLayer);

            if (wallClimbChecks[0])
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up * 0.5f, transform.forward, out hit, 1, wallClimbLayer))
                {
                    wallClimbChecks[5] = true;
                    wallClimbTargetPos = hit.point;
                    wallClimbNormal = hit.normal;
                }
       
            }
            else
            {
                wallClimbNormal = Vector3.zero;
                wallClimbTargetPos = Vector3.zero;
            }
        }
        #endregion

        #region Dash
        /*
        void OnDash(bool performed)
        {
            if (performed && !dashTimer.IsRunning && !dashCooldownTimer.IsRunning && !glideTimer.IsRunning)
            {
                dashTimer.Start();
                glideStamina.StartGlide();
            }
            else if (!performed && dashTimer.IsRunning)
            {
                dashTimer.Stop();
                glideStamina?.StopGlide();
            }
            else
            {
                playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }
            
        }
        */
        #endregion

        #region Jump
        void OnJump(bool performed)
        {
            if (wallClimbimg)
            {
                OnWallClimb(true);
                return;
            }
            
            if (performed && groundChecker.IsGrounded)
            {
                remainingJumps = jumpCount;
            }

            if (performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && remainingJumps > 0)
            {
                remainingJumps--;
                jumpTimer.Start();
                AudioManager.instance.PlayOneShot(FMODEvents.instance.playerJump, this.transform.position);
            }
            else if (!performed && jumpTimer.IsRunning)
            {
                jumpTimer.Stop();
            }
        }
        public void HandleJump()
        {
            // if not jumping and grounded, keep jump velocity at 0
            if (!jumpTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpVelocity = ZeroF;
                //jumpTimer.Stop();
                return;
            }

            // if jumping or falling calculate velocity
            if (!jumpTimer.IsRunning)
            {
                // Gravity takes over
                jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
                jumpVelocity = Mathf.Clamp(jumpVelocity, -maxFallSpeed, maxFallSpeed);
            }

            // Apply velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
        }
        #endregion

        #region Glide
        public void OnGlide(bool performed)
        {
            if (performed)
            {
                if (!glideTimer.IsRunning && !groundChecker.IsGrounded)
                {
                        glideTimer.Start();
                        glideBoost = 0;
                        jumpTimer.Stop();
                        glideStamina.StartGlide();
                        // Enable the gliding mesh and disable the default mesh
                        glidingMesh.SetActive(true);
                        defaultMesh.SetActive(false);
                        
                }
                
            }
            else if (!performed && glideTimer.IsRunning)
            {
                glideStamina?.StopGlide();
                glideTimer.Stop();
                // Revert back to the default mesh
                glidingMesh.SetActive(false);
                defaultMesh.SetActive(true);

            }
        }
        public void HandleGlide()
        {
            if (glideBoost > 1)
            {
                glideBoost -= glideBoostDecayRate;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x * glideBoost, -glideFallSpeed, rb.linearVelocity.z * glideBoost);
            }
            else
            {
                glideBoost = 1;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -glideFallSpeed, rb.linearVelocity.z);
            }
            if (groundChecker.IsGrounded)
            {
                glideTimer.Stop();
                glideStamina?.StopGlide();
                glidingMesh.SetActive(false);
                defaultMesh.SetActive(true);
            }
        }
        #endregion
        
        #region Attack
        void OnAttack()
        {
            if (!attackTimer.IsRunning)
            {
               
                attackTimer.Start();
            }
            
        }

        public void Attack()
        {
            Vector3 attackPos = transform.position + transform.forward * attackDistance;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackDistance);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.playerAttack, this.transform.position);
            foreach (var hit in hitEnemies)
            {
                // Handle standard enemies
                if (hit.CompareTag("Enemy"))
                {
                    if(hit.TryGetComponent<Health>(out Health enemyHealth))
                    {
                        enemyHealth.TakeDamage(attackDamage, knockbackTime);
                    }
                }
                // Handle environmental objects with the "Destructible" tag
                else if (hit.CompareTag("Destructable"))
                {
                    if (hit.TryGetComponent<FractureObject>(out FractureObject fractureObject))
                    {
                        // 1. Trigger the explosion
                        fractureObject.Explode();
                    }
                }
            }
        }

        void OnSpinAttack()
        {
            if (!spinAttackTimer.IsRunning)
            {
                spinAttackTimer.Start();
            }
        }

        public void SpinAttack()
        {
            Vector3 attackPos = transform.position;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, spinAttackDistance);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.playerAttack, this.transform.position);
            foreach (var enemy in hitEnemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemy.GetComponent<Health>().TakeDamage(spinAttackDamage, knockbackTime);
                }
            }
        }
        #endregion

        #region Echolocation
        void OnEcho(bool performed)
        {
            if (!echoTimer.IsRunning && currentEchoCharges > 0)
            {
                currentEchoCharges--;
                echoTimer.Start();
                AudioManager.instance.PlayOneShot(FMODEvents.instance.playerEcolocation, this.transform.position);
                UpdateEchoChargeUI();

                // Start regeneration when we use our first charge
                if (!isRegenerating && currentEchoCharges < maxEchoCharges)
                {
                    StartCoroutine(RegenerateCharge());
                }
                
            }
        }
        public void HandleEcho()
        {
            if (echoCooldown > 0 && echoTimer.IsRunning && currentEchoCharges >= 0)
            {
                detectionParticle.Play();
                Collider[] detectedObjects = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);
                foreach (Collider collider in detectedObjects)
                {
                    if (collider.CompareTag("Enemy"))
                    {
                        EnableEchoEffect(true);
                    }
                    else if (collider.CompareTag("Collectible"))
                    {
                        EnableEchoEffect(true);
                    }
                }
                Invoke(nameof(DisableEchoEffect), echoDuration);
            }
        }

     
        private void EnableEchoEffect(bool state)
        {
            if (echoRendererFeature != null)
            {
                echoRendererFeature.SetActive(state);
            }
        }
        private void DisableEchoEffect()
        {
            EnableEchoEffect(false);
        }
        
        private IEnumerator RegenerateCharge()
        {
            isRegenerating = true;
            float timer = 0;

            while (currentEchoCharges < maxEchoCharges)
            {
                timer += Time.deltaTime;
                echoChargeUI.fillAmount = (float)currentEchoCharges / maxEchoCharges + (timer / chargeRegenerationTime) * (1f / maxEchoCharges);

                if (timer >= chargeRegenerationTime)
                {
                    currentEchoCharges++;
                    timer = 0;
                    UpdateEchoChargeUI();
                }
                yield return null;
            }

            isRegenerating = false;
        }

        
        void UpdateEchoChargeUI()
        {
            echoChargeUI.fillAmount = (float)currentEchoCharges / maxEchoCharges;
        }
        #endregion

        #region Interact
        void OnInteract(bool performed)
        {
            if(performed)
            {
                foreach (var interactable in FindObjectsByType<Interactable>(FindObjectsSortMode.InstanceID))
                {
                    if (Vector3.Distance(transform.position, interactable.transform.position) < interactDistance)
                    {
                        interactable.Interact();
                        break;
                    }
                }
            }
        }
        #endregion
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * attackDistance, attackDistance);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spinAttackDistance);
     
          
            if (Application.isPlaying)
            {
                Gizmos.color = Color.grey;
                if(wallClimbChecks[0]) Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + transform.forward * wallCheckDist + transform.up, 0.5f);
         
                Gizmos.color = Color.yellow;
                if(wallClimbChecks[1]) Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position + transform.up * 2, transform.forward * 1);
         
                /*
                Gizmos.color = Color.yellow;
                if(wallClimbChecks[2]) Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position - transform.up * 0.6f, transform.forward * 1);

                Gizmos.color = Color.yellow;
                if(wallClimbChecks[3]) Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position + transform.right * 1 + transform.up * 0.5f, transform.forward);
                */
                
                Gizmos.color = Color.yellow;
                if(wallClimbChecks[4]) Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position - transform.right * 1 + transform.up * 0.5f, transform.forward);
         
                Gizmos.color = Color.magenta;
                if(wallClimbNormal != Vector3.zero) Gizmos.DrawRay(wallClimbTargetPos, wallClimbNormal);
                
            }
        }
        
        public void LoadData(GameData data) => this.transform.position = data.playerPosition;
        public void SaveData(GameData data) => data.playerPosition = this.transform.position;
        void OnEnable()
        {
            input.Jump += OnJump;
            //input.Dash += OnDash;
            input.Echo += OnEcho;
            input.Wallclimb += OnWallClimb;
            input.Glide += OnGlide;
            input.Attack += OnAttack;
            input.SpinAttack += OnSpinAttack;
            input.interact += OnInteract;
            
        }

        void OnDisable()
        {
            input.Jump -= OnJump;
            // input.Dash -= OnDash;
            input.Echo -= OnEcho;
            input.Wallclimb -= OnWallClimb;
            input.Glide -= OnGlide;
            input.Attack -= OnAttack;
            input.SpinAttack -= OnSpinAttack;
            input.interact -= OnInteract;

        }
        
        [Header("Swim Settings")]
        [SerializeField] float swimSpeed = 4f;
        [SerializeField] float swimLevelOffset = 1.2f; // How deep the player sits in the water
        [SerializeField] float waterSurfaceY; // Stores the height of the water
        [SerializeField] GameObject objectActiveInWater;
        public bool InWater { get; private set; }
        
        private void OnTriggerEnter(Collider other)
        {
            // Detect entering water
            if (other.CompareTag("Water"))
            {
                InWater = true;
                // Assume the top of the collider is the water surface
                waterSurfaceY = other.bounds.max.y;
                
                if (objectActiveInWater != null) 
                    objectActiveInWater.SetActive(true);
                // ----------------
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Detect leaving water
            if (other.CompareTag("Water"))
            {
                InWater = false;
                
                // --- ADD THIS ---
                if (objectActiveInWater != null) 
                    objectActiveInWater.SetActive(false);
            }
        }

        public void HandleSwimming()
        {
            // 1. Determine Movement Direction (Same as normal movement)
            var adjustedDirection = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * movement;
    
            // 2. Apply Swim Speed
            if (adjustedDirection.magnitude > 0f)
            {
                HandleRotation(adjustedDirection);
                // Move horizontally using swimSpeed instead of moveSpeed
                Vector3 velocity = adjustedDirection * (swimSpeed * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
                SmoothSpeed(adjustedDirection.magnitude);
            }
            else
            {
                SmoothSpeed(0f);
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            // 3. Handle Floating (Buoyancy)
            // Smoothly move the player's Y position to the water surface minus the offset
            float targetY = waterSurfaceY - swimLevelOffset;
            Vector3 currentPos = transform.position;
    
            // Lerp specifically on the Y axis to create a floating effect
            float newY = Mathf.Lerp(currentPos.y, targetY, Time.fixedDeltaTime * 5f);
            transform.position = new Vector3(currentPos.x, newY, currentPos.z);
    
            // Kill vertical velocity so gravity doesn't pull us down
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            UpdateSound(); // Optional: You might want a different sound for swimming later
        }
    }
}