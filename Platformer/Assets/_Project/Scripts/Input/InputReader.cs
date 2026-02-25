using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

namespace Platformer
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Platformer/InputReader")]
    public class InputReader : ScriptableObject, IPlayerActions, IMenuActions
    {
        [field: Header("Player Actions Events")]
        PlayerInputActions inputActions;
        public Vector3 Direction => inputActions.Player.Move.ReadValue<Vector2>();
        public bool CounterPressed { get; set; }

        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2> Look = delegate { };
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction<bool> Dash = delegate { };
        public event UnityAction<bool> SonarPulse = delegate { };
        public event UnityAction<bool> Wallclimb = delegate { };
        public event UnityAction<bool> Glide = delegate { };
        public event UnityAction LightAttack = delegate { };
        public event UnityAction HeavyAttack = delegate { };
        public event UnityAction<bool> interact = delegate { };
        public event UnityAction<bool> submit = delegate { };
        public event UnityAction Paused = delegate { };
        
        public bool IsJumpKeyPressed = false;
        
        public event UnityAction Counter = delegate { }; 

        public void OnCounter(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                Counter.Invoke();
                CounterPressed = true;
            }
        }
        public void OnMove(InputAction.CallbackContext context)
        {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
           Look.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLightAttack(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                LightAttack.Invoke();
            }
        }

        public void OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                HeavyAttack.Invoke();
            }
        }
        public void OnJump(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    IsJumpKeyPressed = true; // 2. Set to TRUE when pressed
                    Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    IsJumpKeyPressed = false; // 3. Set to FALSE when released
                    Jump.Invoke(false);
                    break;
            }
        }
        public void OnDash(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Dash.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Dash.Invoke(false);
                    break;
            }
        }

        public void OnSonarPulse(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    SonarPulse.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    SonarPulse.Invoke(false);
                    break;
            }
        }
        public void OnWallclimb(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Wallclimb.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Wallclimb.Invoke(false);
                    break;
            }
        }
        public void OnGlide(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Glide.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Glide.Invoke(false);
                    break;
            }
        }
        public void OnInteract(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    interact.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    interact.Invoke(false);
                    break;
            }
        }
        public void OnSubmit(InputAction.CallbackContext context) { }
        public void EnablePlayerActions()
        {
            if (inputActions == null) 
            {
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }
            inputActions.Enable();
        }

        void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }
            inputActions.Menu.Escape.performed += OnEscape;
            EnablePlayerActions();
        }
        void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Menu.Escape.performed -= OnEscape;
                inputActions.Disable();
            }
        }
        public void OnEscape(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                Paused.Invoke();
            }
        }
    }
}