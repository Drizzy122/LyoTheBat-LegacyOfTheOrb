using UnityEngine;

namespace Platformer
{
    public class GroundChecker : MonoBehaviour
    {
        [Header("Ground Check Settings")]
        [SerializeField] float groundDistance = 0.3f;
        [SerializeField] float radius = 0.3f;
        [SerializeField] LayerMask groundLayers;

        [Header("Debug")]
        [SerializeField] bool showGizmoAlways = true;

        public bool IsGrounded;

        Vector3 CheckPosition => transform.position + Vector3.down * groundDistance;

        void Update()
        {
            IsGrounded = Physics.CheckSphere(CheckPosition, radius, groundLayers, QueryTriggerInteraction.Ignore);
        }

        void OnDrawGizmos()
        {
            if (!showGizmoAlways) return;
            DrawGizmo();
        }

        void OnDrawGizmosSelected()
        {
            if (showGizmoAlways) return;
            DrawGizmo();
        }

        void DrawGizmo()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(CheckPosition, radius);
        }
    }
}
