using Unity.Cinemachine;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.InputSystem; // Added this to read the raw mouse data!

namespace Platformer
{
    public class CameraManager : ValidatedMonoBehaviour
    {
        public static CameraManager instance { get; private set; }
        
        [Header("References")]
        [SerializeField, Anywhere] CinemachineInputAxisController axisController;
        [SerializeField, Anywhere] InputReader inputReader;

        public InputReader InputReader => inputReader;

        /// <summary>
        /// Freezes/unfreezes camera orbit. Cinemachine reads the Look action
        /// directly, so disabling this component alone doesn't stop the camera —
        /// the axis controller itself must be switched off (used by dialogue).
        /// </summary>
        public void SetLookInputEnabled(bool value)
        {
            if (axisController != null) axisController.enabled = value;
        }
        
        [Header("Settings")]
        [SerializeField, Range(0.1f, 10f)] float horizontalSpeed = 1f;
        [SerializeField, Range(0.1f, 10f)] float verticalSpeed = 1f;

        // --- UI VARIABLES ---
        private float controllerSensitivity = 1f;
        private float mouseSensitivity = 1f;
        private bool invertControllerY = false;
        private bool invertMouseY = false;
        
        // Bulletproof Hardware Tracker
        private bool isActuallyUsingMouse = true;
        
        void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one Camera Manager in the scene.");
            }
            instance = this;
        }
        
        void Update()
        {
            // 1. Physically check if the mouse is moving on the desk
            if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            {
                isActuallyUsingMouse = true;
            }
            else if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f)
            {
                isActuallyUsingMouse = false;
            }

            CameraMovement();
        }

        void CameraMovement()
        {
            if (axisController != null && axisController.Controllers.Count >= 2)
            {
                // 2. Pick the right sensitivity based on our hardware check
                float activeMultiplier = isActuallyUsingMouse ? mouseSensitivity : controllerSensitivity;

                // 3. Apply to Horizontal Camera (X Axis)
                var xController = axisController.Controllers[0];
                xController.Input.Gain = horizontalSpeed * activeMultiplier; 
                axisController.Controllers[0] = xController;
                
                // 4. Apply to Vertical Camera (Y Axis)
                var yController = axisController.Controllers[1];
                bool activeInvert = isActuallyUsingMouse ? invertMouseY : invertControllerY;
                
                //Debug.Log($"Using Mouse: {isActuallyUsingMouse} | Mouse Invert Toggle: {invertMouseY} | Resulting Invert: {activeInvert}");

                // 5. Apply the Invert Math
                float yDirection = activeInvert ? -1f : 1f;
                yController.Input.Gain = verticalSpeed * activeMultiplier * yDirection;   
                
                axisController.Controllers[1] = yController;
            }
        }

        // --- PUBLIC METHODS FOR SETTINGS MANAGER ---
        public void SetControllerSensitivity(float val) { controllerSensitivity = val; }
        public void SetMouseSensitivity(float val) { mouseSensitivity = val; }
        public void SetControllerInvertY(bool val) { invertControllerY = val; }
        public void SetMouseInvertY(bool val) { invertMouseY = val; }

        void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}