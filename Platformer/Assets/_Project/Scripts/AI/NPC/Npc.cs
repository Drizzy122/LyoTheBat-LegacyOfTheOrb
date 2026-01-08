using KBCore.Refs;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace Platformer
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
    public class Npc : Entity
    {
        [SerializeField, Self] NavMeshAgent agent;
        [SerializeField, Self] PlayerDetector playerDetector;
        [SerializeField, Child] Animator animator;
        
        [SerializeField] float timeBetweenIdle = 3f;
        [SerializeField] float timeBetweenWander = 5f; //

        [SerializeField] float wanderRadius = 10f;
        [SerializeField] public float rotationSpeed = 5f; // Adjust this in the Inspector

        
        StateMachine stateMachine;
        CountdownTimer idleTimer;
        CountdownTimer wanderTimer;

        void OnValidate() => this.ValidateRefs();
        
        void Start()
        {
            idleTimer = new CountdownTimer(timeBetweenIdle);
            wanderTimer = new CountdownTimer(timeBetweenWander); // Timer for Wander state

            stateMachine = new StateMachine();
            
            var idleState = new NpcIdleState(this, animator, agent, playerDetector.Player, idleTimer);
            var wanderState = new NpcWonderState(this, animator, agent, wanderRadius, wanderTimer);
            var interactState = new NpcInteractState(this, animator);
            
            At(idleState, interactState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));  
            At(interactState, idleState, new FuncPredicate(() => !playerDetector.CanDetectPlayer())); 
            
            At(idleState, wanderState, new FuncPredicate(() => !idleTimer.IsRunning && !playerDetector.CanDetectPlayer()));  // Idle → Wander
            At(wanderState, idleState, new FuncPredicate(() => !wanderTimer.IsRunning || playerDetector.CanDetectPlayer()));  // Wander → Idle

            // Add a global transition to return to Idle from any state if the player is detected
            Any(idleState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
            
            stateMachine.SetState(idleState);
        }
        
        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
       

        public void ChangeStateToIdle()
        {
            stateMachine.SetState(new NpcIdleState(this, animator, agent, playerDetector.Player, idleTimer));
        }

        public void ChangeStateToWonder()
        {
            stateMachine.SetState(new NpcWonderState(this, animator, agent, wanderRadius, wanderTimer));
        }

        void Update()
        {
            stateMachine.Update();
            idleTimer.Tick(Time.deltaTime);  // Update Idle timer
            wanderTimer.Tick(Time.deltaTime); // Update Wander timer
        }

        void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }
        
        void OnEnable()
        {
            GameEventsManager.instance.inputEvents.onSubmitPressed += SubmitPressed;
        }

        void OnDisable()
        {
            GameEventsManager.instance.inputEvents.onSubmitPressed -= SubmitPressed;
        }


        private void SubmitPressed(InputEventContext inputEventContext)
        {
            if (playerDetector.CanDetectPlayer())
            {
                ChangeStateToInteract();
            }
        }
        
        public void ChangeStateToInteract()
        {
            stateMachine.SetState(new NpcInteractState(this, animator));
        }

    }
}