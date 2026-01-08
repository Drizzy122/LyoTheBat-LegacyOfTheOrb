using UnityEngine;

namespace Platformer
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] float groundDistance = 0.3f; // Distance to check the ground
        [SerializeField] LayerMask groundLayers; // Ground layers to include
        public bool IsGrounded;

        void Update()
        {
            // Adjust starting point slightly above the object's position
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            // Perform raycast downwards
            IsGrounded = Physics.Raycast(origin, Vector3.down, groundDistance + 0.1f, groundLayers);
        }

        private void OnDrawGizmosSelected()
        {
            // Use Gizmos to visualize the ground check
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.down * groundDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * groundDistance, 0.1f);
        }
    }
}