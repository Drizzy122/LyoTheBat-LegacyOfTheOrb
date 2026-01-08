using Unity.Cinemachine;
using KBCore.Refs;
using UnityEngine;

namespace Platformer
{
    public class CameraManager : ValidatedMonoBehaviour
    {
        public static CameraManager instance { get; private set; }
        [Header("References")] 
        // We reference the Axis Controller directly
        [SerializeField, Anywhere] CinemachineInputAxisController axisController;
        
        [Header("Settings")]
        [SerializeField, Range(0.1f, 10f)] float horizontalSpeed = 1f;
        [SerializeField, Range(0.1f, 10f)] float verticalSpeed = 1f;
        
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
            CameraMovement();
        }

        void CameraMovement()
        {
            if (axisController != null && axisController.Controllers.Count >= 2)
            {
                var xController = axisController.Controllers[0];
                xController.Input.Gain = horizontalSpeed; // Access Gain via .Input
                axisController.Controllers[0] = xController;
                
                var yController = axisController.Controllers[1];
                yController.Input.Gain = verticalSpeed;   // Access Gain via .Input
                axisController.Controllers[1] = yController;
            }
        }

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