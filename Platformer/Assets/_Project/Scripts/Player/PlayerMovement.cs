using KBCore.Refs;
using ImprovedTimers;
using UnityEngine;
using UnityUtils.StateMachine;

namespace Platformer
{
    public class PlayerMovement : ValidatedMonoBehaviour
    {
        #region Variables
        [field: Header("References")] 
        [field: SerializeField, Anywhere] InputReader input;
        [field: SerializeField, Self] Rigidbody rb;
        [field: SerializeField, Self] GroundChecker groundChecker;
        [field: SerializeField, Self] Animator animator;
        [field: SerializeField, Self] Health playerHealth;
        [field: SerializeField, Self]  public PlayerCombat combat;
        [field: SerializeField, Self]  public CommandManager commandManager;
        [field: SerializeField, Self] GlideStamina glideStamina;
        [field: SerializeField, Self] public PlayerInteraction interaction;
        
        [Header("Movement Settings")]
        [field: SerializeField] [Range(0,10)] float moveSpeed = 10f;
        [field: SerializeField] [Range(0,20)] float sprintSpeed = 18f;
        [field: SerializeField] [Range(0,1000)] float rotationSpeed = 1000f;
        public bool IsSprinting { get; private set; }
        [field: SerializeField] float smoothTime = 0.2f;
        
        [field: Header("Jump Settings")] 
        [field: SerializeField] float jumpForce = 10f;
        [field: SerializeField] float jumpDuration = 0.5f;
        [field: HideInInspector] int jumpCount = 2;
        [field: HideInInspector] int remainingJumps = 2;
        [field: SerializeField] float gravityMultiplier = 3f;
        [field: SerializeField] int maxFallSpeed = 10;

        [field: Header("Dash Settings")] 
        [field: SerializeField] float dashForce = 10f;
        [field: SerializeField] float dashDuration = 1f;
        
        [field: Header("Glide Settings")] 
        [field: SerializeField] float glideMoveSpeed = 2;
        [field: SerializeField] float glideFallSpeed = 0.1f;
        
        [field: SerializeField] public float glideBoost = 1;
        [field: SerializeField] float glideBoostDecayRate = 0.02f;

        [Header("Sprint Glide Entry Boost (independent of orb/ring boost)")]
        [field: SerializeField] float sprintGlideBoost = 1.5f;
        [field: SerializeField] float sprintGlideBoostDecay = 0.02f;
        float currentSprintBoost = 1f;
        
        [field: SerializeField] GameObject defaultMesh;  
        [field: SerializeField] GameObject glidingMesh;
        
        [field: Header("Wall Climb Settings")]
        [field: SerializeField] float wallCheckDist = 1f;
        [field: SerializeField] float wallClimbMoveSpeed = 5f;
        [field: SerializeField] LayerMask wallClimbLayer;
        [HideInInspector] public bool wallClimbimg;
        [HideInInspector] bool[] wallClimbChecks;
        [HideInInspector] public Vector3 wallClimbPos;
        Vector3 wallClimbNormal;
        Vector3 wallClimbTargetPos;
       
        
        [Header("Swim Settings")]
        [SerializeField] float swimSpeed = 4f;
        [SerializeField] float swimLevelOffset = 1.2f; 
        [SerializeField] float waterSurfaceY;
        [SerializeField] GameObject objectActiveInWater;
        public bool InWater { get; private set; }

        [field: Header("More variables")]
        const float ZeroF = 0f;
        float currentSpeed;
        float velocity;
        float jumpVelocity;
        float dashVelocity = 1f;
        Vector3 movement;
        Transform mainCam;
        
        [field: Header("Animation Cooldown Settings")]
        [field: SerializeField] float hurtDuration = 0.4f;
        [field: SerializeField] float jumpCooldown = 0f;
        [field: SerializeField] public float glideCoolDown = 0.5f;
        [field: SerializeField] float dashCooldown = 0.5f;

        [field: Header("Timers")]
        CountdownTimer jumpTimer;
        CountdownTimer jumpCooldownTimer;
        CountdownTimer dashTimer;
        CountdownTimer dashCooldownTimer;
        CountdownTimer glideTimer;
        CountdownTimer hurtTimer;
      

        StateMachine stateMachine;
        
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
        void Start() => input.EnablePlayerActions();

        void Update()
        {
            movement = new Vector3(input.Direction.x, 0f, input.Direction.y);
            stateMachine.Update();
            UpdateAnimator();
        }
        void FixedUpdate()
        {
            stateMachine.FixedUpdate();
            WallClimbCheck();
        }
        void UpdateAnimator() => animator.SetFloat(Speed, currentSpeed);
       

        #region StateMachine
        private void SetupStateMachine()
        {
            // State Machine
            stateMachine = new StateMachine();

            // Declare states
            var locomotionState = new LocomotionState(this, animator);
            var sprintState = new SprintState(this, animator);
            var jumpState = new JumpState(this, animator);
            var doubleJumpState = new DoubleJumpState(this, animator);
            var glideState = new GlideState(this, animator);
            var dashState = new DashState(this, animator);
            var attackState = new AttackState(this, animator);
            var spinAttackState = new BlastAttackState(this, animator);
            var deathState = new DeathState(this, animator, playerHealth);
            var teleportState = new TeleportState(this, animator);
            var wallClimbState = new WallClimbState(this, animator);
            var swimState = new SwimState(this, animator);
            var hurtState = new HurtState(this, animator);

            
            // Define transitions for sprint
            At(locomotionState, sprintState, new FuncPredicate(() => IsSprinting && movement.sqrMagnitude > 0f));
            At(sprintState, locomotionState, new FuncPredicate(() => !IsSprinting || movement.sqrMagnitude <= 0f));
            At(sprintState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning || !groundChecker.IsGrounded));
            At(sprintState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(sprintState, attackState, new FuncPredicate(() => combat.IsAttacking));
            At(sprintState, spinAttackState, new FuncPredicate(() => combat.IsBlastAttacking));
            At(sprintState, swimState, new FuncPredicate(() => InWater));

            // Define transitions for jump
            At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
            At(locomotionState, jumpState, new FuncPredicate(() => !groundChecker.IsGrounded));

            // Define transitions for double jump
            At(doubleJumpState, glideState, new FuncPredicate(() => glideTimer.IsRunning && !InWater));;
            At(jumpState, doubleJumpState, new FuncPredicate(() => jumpTimer.IsRunning && remainingJumps <= jumpCount - 2));
            At(doubleJumpState, dashState, new FuncPredicate(() => dashTimer.IsRunning));

            // Define transitions for Dash 
            At(locomotionState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(glideState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(jumpState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(dashState, jumpState, new FuncPredicate(() => !dashTimer.IsRunning));
            
            // Define transitions for attack
            At(locomotionState, attackState, new FuncPredicate(() => combat.IsAttacking));
            At(attackState, locomotionState, new FuncPredicate(() => !combat.IsAttacking));;

            At(locomotionState, spinAttackState, new FuncPredicate(() => combat.IsBlastAttacking));
            At(spinAttackState, locomotionState, new FuncPredicate(() => !combat.IsBlastAttacking));

            // Enable Spin Attack while jumping
            At(jumpState, spinAttackState, new FuncPredicate(() => combat.IsBlastAttacking));
            At(spinAttackState, jumpState, new FuncPredicate(() => !combat.IsBlastAttacking || jumpTimer.IsRunning));
            
            // Define transitions for glide
            At(jumpState, glideState, new FuncPredicate(() => glideTimer.IsRunning && !InWater));
            At(dashState, glideState, new FuncPredicate(() => glideTimer.IsRunning));
            At(glideState, jumpState, new FuncPredicate(() => !glideTimer.IsRunning));
            
            // Define transitions for wall climb
            At(jumpState, wallClimbState, new FuncPredicate(() => wallClimbimg));
            At(doubleJumpState, wallClimbState, new FuncPredicate(() => wallClimbimg));
            At(wallClimbState, doubleJumpState, new FuncPredicate(() => !wallClimbimg));
            
            // Definne transition for teleportation
             At(teleportState, locomotionState, new FuncPredicate(() => !interaction.isTeleporting));
            
             At(locomotionState, swimState, new FuncPredicate(() => InWater));
             At(swimState, locomotionState, new FuncPredicate(() => !InWater));
             
            
             At(jumpState, swimState, new FuncPredicate(() => InWater));
             At(doubleJumpState, swimState, new FuncPredicate(() => InWater));
             At(glideState, swimState, new FuncPredicate(() => InWater));
             
             // Jump out: If we press jump while swimming (uses the logic we set up in OnJump)
             At(swimState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
             
            
             // Set initial state
            Any(teleportState, new FuncPredicate(() => interaction.isTeleporting));
            Any(hurtState, new FuncPredicate(() => hurtTimer.IsRunning && !playerHealth.isDead));
            Any(deathState, new FuncPredicate(() => playerHealth.isDead));
            Any(locomotionState, new FuncPredicate(ReturnToLocomotionState));
            
            stateMachine.SetState(locomotionState);
        }

        bool ReturnToLocomotionState()
        {
            return groundChecker.IsGrounded
                   && !InWater
                   && !playerHealth.isDead
                   && !combat.IsAttacking
                   && !combat.IsBlastAttacking
                   && !jumpTimer.IsRunning
                   && !dashTimer.IsRunning
                   && !glideTimer.IsRunning
                   && !interaction.isTeleporting
                   && !hurtTimer.IsRunning
                   && !(IsSprinting && movement.sqrMagnitude > 0f);

        }
        void At(IState from, IState to, FuncPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, FuncPredicate condition) => stateMachine.AddAnyTransition(to, condition);
        #endregion

        #region Timers
        
        void SetupTimers()
        {
            // Setup timers
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);
            glideTimer = new CountdownTimer(glideCoolDown);
            dashTimer = new CountdownTimer(dashDuration);
            dashCooldownTimer = new CountdownTimer(dashCooldown);

            jumpTimer.OnTimerStart += () => jumpVelocity = jumpForce;
            jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();
            hurtTimer = new CountdownTimer(0f);
            dashTimer.OnTimerStart += () => dashVelocity = dashForce;
            dashTimer.OnTimerStop += () =>
            {
                dashVelocity = 1f;
                dashCooldownTimer.Start();
            };
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
                if (glideTimer.IsRunning) adjustedDirection *= (glideMoveSpeed * currentSprintBoost);
                HandleHorizontalMovement(adjustedDirection * glideBoost);
                SmoothSpeed(adjustedDirection.magnitude);
            }
            else
            {
                SmoothSpeed(ZeroF);

                // Reset horizontal velocity for a snappy stop
                rb.linearVelocity = new Vector3(ZeroF, rb.linearVelocity.y, ZeroF);
            }
            
        }
        void HandleHorizontalMovement(Vector3 adjustedDirection)
        {
            // Sprint applies on the ground and through the full jump arc (ascent + descent).
            // Glide overrides it so the orb/ring boost system stays in charge while gliding.
            bool canSprint = IsSprinting && !glideTimer.IsRunning;
            float activeSpeed = canSprint ? sprintSpeed : moveSpeed;
            Vector3 velocity = adjustedDirection * (activeSpeed * dashVelocity);
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
        // NEW: Gets the exact direction the player is trying to move relative to the camera
        public Vector3 GetAdjustedMovementDirection()
        {
            var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;
            return adjustedDirection.normalized;
        }
        
        public void StopMovement()
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            // You'll need to use reflection or just set currentSpeed to 0 if it's accessible!
            // Since currentSpeed is private in PlayerController, we can just set it here:
            currentSpeed = 0f; 
        }
        
        public void OnSprint(bool performed)
        {
            IsSprinting = performed;
        }

        
        public void HandleSwimming()
        {
            if (jumpTimer.IsRunning) return;

            // 1. Determine Movement Direction (Same as normal movement)
            var adjustedDirection = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * movement;
    
            // 2. Apply Swim Speed
            if (adjustedDirection.magnitude > 0f)
            {
                HandleRotation(adjustedDirection);
                // Move horizontally using swimSpeed instead of moveSpeed
                Vector3 velocity = adjustedDirection * swimSpeed;
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
        }
        void HandlePlayerHurt(float knockbackTime)
        {
            if (!playerHealth.isDead)
            {
                // Use knockbackTime if the enemy provided one, otherwise use our new short hurtDuration!
                float stunDuration = knockbackTime > 0 ? knockbackTime : hurtDuration;
                hurtTimer.Reset(stunDuration);
                hurtTimer.Start();

                // Interrupt all active timers so HurtState gets clean physics
                jumpTimer.Stop();
                dashTimer.Stop();
                combat.CancelActions();
            }
        }
        
        public bool IsGliding => glideTimer != null && glideTimer.IsRunning;

        // True only while actually sprint-moving (grounded + giving input), not just holding the key.
        public bool IsSprintingActively => IsSprinting && groundChecker.IsGrounded && movement.sqrMagnitude > 0.01f;
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
            
            if (wallClimbChecks[0])
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up * 0.5f, transform.forward, out hit, 1, wallClimbLayer))
                {
                    wallClimbChecks[5] = true;
                    wallClimbTargetPos = hit.point;
                    wallClimbNormal = hit.normal;
                    
                    
                    float dot = Vector3.Dot(transform.forward, -wallClimbNormal);
                    if (!wallClimbimg && movement.magnitude > 0 && !jumpTimer.IsRunning && dot > 0.5f)
                    {
                        wallClimbimg = true;
                        wallClimbPos = hit.point + (hit.normal * 0.2f);
                    }
                }
            }
            else
            {
                wallClimbNormal = Vector3.zero;
                wallClimbTargetPos = Vector3.zero;
                if (wallClimbimg) wallClimbimg = false;
            }
        }
        #endregion

        #region Dash
        
        void OnDash(bool performed)
        {
            if (performed && !dashTimer.IsRunning && !dashCooldownTimer.IsRunning)
            {
                dashTimer.Start();
                
            }
            else if (!performed && dashTimer.IsRunning)
            {
                dashTimer.Stop();
            }
        }
        #endregion

        #region Jump
        void OnJump(bool performed)
        {
            if (wallClimbimg)
            {
                OnWallClimb(true);
                return;
            }
            
            if (performed && (groundChecker.IsGrounded || InWater))
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
            if (InWater) 
            {
                glideTimer.Stop(); 
                return; 
            }
            if (performed)
            {
                if (!glideTimer.IsRunning && !groundChecker.IsGrounded)
                {
                    glideTimer.Start();
                    glideBoost = 0;
                    currentSprintBoost = IsSprinting ? sprintGlideBoost : 1f;
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

            // Decay the sprint-into-glide entry boost back toward 1
            if (currentSprintBoost > 1f)
                currentSprintBoost = Mathf.Max(1f, currentSprintBoost - sprintGlideBoostDecay);

            if (groundChecker.IsGrounded)
            {
                glideTimer.Stop();
                glideStamina?.StopGlide();
                glidingMesh.SetActive(false);
                defaultMesh.SetActive(true);
                currentSprintBoost = 1f;
            }
        }
        #endregion
        

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.grey;
                if(wallClimbChecks[0]) Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + transform.forward * wallCheckDist + transform.up, 0.5f);
         
                Gizmos.color = Color.yellow;
                if(wallClimbChecks[1]) Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position + transform.up * 2, transform.forward * 1);
                
                Gizmos.color = Color.yellow;
                if(wallClimbChecks[4]) Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position - transform.right * 1 + transform.up * 0.5f, transform.forward);
         
                Gizmos.color = Color.magenta;
                if(wallClimbNormal != Vector3.zero) Gizmos.DrawRay(wallClimbTargetPos, wallClimbNormal);
                
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                InWater = true;
                waterSurfaceY = other.bounds.max.y;
                
                if (objectActiveInWater != null) 
                    objectActiveInWater.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                InWater = false;
                if (objectActiveInWater != null) 
                    objectActiveInWater.SetActive(false);
            }
        }
        
        /*
       private void OnCounter()
       {
           if (!playerHealth.isDead && !hurtTimer.IsRunning)
           {
               animator.CrossFade("Counter", 0.1f);
               combat.CounterCheck();
           }
       }*/
        
        void OnEnable()
        {
            input.Sprint += OnSprint;
            input.Jump += OnJump;
            input.Dash += OnDash;
            input.Wallclimb += OnWallClimb;
            input.Glide += OnGlide;
            playerHealth.OnHit += HandlePlayerHurt;
            //input.Counter += OnCounter;

        }
        void OnDisable()
        {
            input.Jump -= OnJump;
            input.Sprint -= OnSprint;
            input.Dash -= OnDash;
            input.Wallclimb -= OnWallClimb;
            input.Glide -= OnGlide;
            playerHealth.OnHit -= HandlePlayerHurt;
            // input.Counter -= OnCounter;
        }
    }
}