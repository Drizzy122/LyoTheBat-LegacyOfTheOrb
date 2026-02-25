using UnityEngine;

namespace Platformer
{
    public class EnemyDetection : MonoBehaviour
    {
        [field: Header("Targeting Settings")]
        public LayerMask layerMask;
        
        [SerializeField] private Vector3 inputDirection;
        [SerializeField] private Transform currentTarget;

        // We will call this from your PlayerController's Update loop
        public void ScanForEnemies(Vector3 inputDir)
        {
            inputDirection = inputDir;

            // If we aren't pressing anything, cast the laser straight ahead
            if (inputDirection == Vector3.zero)
            {
                inputDirection = transform.forward;
            }

            RaycastHit info;

            // The exact Mix and Jam SphereCast! (Start, Radius, Direction, out hit, Distance, Layer)
            if (Physics.SphereCast(transform.position, 3f, inputDirection, out info, 10f, layerMask))
            {
                if (info.collider.CompareTag("Enemy") || info.collider.CompareTag("Destructable"))
                {
                    currentTarget = info.collider.transform;
                }
            }
            else
            {
                // Clear the target if we point away into empty space
                currentTarget = null; 
            }
        }

        // Getters to match the Mix & Jam setup
        public Transform CurrentTarget()
        {
            return currentTarget;
        }

        public void SetCurrentTarget(Transform target)
        {
            currentTarget = target;
        }

        public float InputMagnitude()
        {
            return inputDirection.magnitude;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawRay(transform.position, inputDirection * 5f); 
            Gizmos.DrawWireSphere(transform.position, 1f);
            if (CurrentTarget() != null)
            {
                Gizmos.DrawSphere(CurrentTarget().position + (Vector3.up * 1f), 0.5f);
            }
        }
    }
}