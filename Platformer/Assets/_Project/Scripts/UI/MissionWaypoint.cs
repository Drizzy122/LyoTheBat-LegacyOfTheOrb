using UnityEngine;
using UnityEngine.UIElements;

namespace Platformer
{
    public class MissionWaypoint : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("Add waypoints in order. The last one is the final destination.")]
        [SerializeField] Transform[] targets;
        [Tooltip("How close the player must be to advance to the next target.")]
        [SerializeField] float reachRadius = 3f;
        public Vector3 offset;

        [Header("Player")]
        [Tooltip("Drag the Player transform here so distance is measured from the player, not this object.")]
        [SerializeField] Transform player;

        [Header("UI Toolkit")]
        public UIDocument uiDocument;

        // UI Elements queried from the document
        private VisualElement waypointContainer;
        private Label meterLabel;

        private int currentIndex = 0;

        private Transform CurrentTarget
        {
            get
            {
                if (targets == null || targets.Length == 0) return null;
                if (currentIndex >= targets.Length) return null;
                return targets[currentIndex];
            }
        }

        private bool IsOnFinalTarget => targets != null && currentIndex == targets.Length - 1;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            waypointContainer = root.Q<VisualElement>("WaypointContainer");
            meterLabel = root.Q<Label>("MeterLabel");

            // Hidden until Show() is called or targets are assigned at runtime
            Hide();
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>Show the waypoint and start tracking from the first target.</summary>
        public void Show()
        {
            currentIndex = 0;
            if (waypointContainer != null)
                waypointContainer.style.display = DisplayStyle.Flex;
        }

        /// <summary>Immediately hide the waypoint without changing the target list.</summary>
        public void Hide()
        {
            if (waypointContainer != null)
                waypointContainer.style.display = DisplayStyle.None;
        }

        // ── Update ───────────────────────────────────────────────────────────

        private void Update()
        {
            if (waypointContainer == null) return;

            // Nothing to track → stay hidden
            if (CurrentTarget == null)
            {
                Hide();
                return;
            }

            // Ensure visible while tracking
            if (waypointContainer.style.display == DisplayStyle.None) return;

            if (float.IsNaN(waypointContainer.layout.width)) return;

            // Use player position for all world-space checks; fall back to this transform if unassigned
            Vector3 playerPos = player != null ? player.position : transform.position;
            Vector3 playerFwd = player != null ? player.transform.forward : transform.forward;

            float dist = Vector3.Distance(CurrentTarget.position, playerPos);

            if (!IsOnFinalTarget)
            {
                // Advance through intermediate targets when close enough
                if (dist <= reachRadius)
                    AdvanceToNextTarget();
            }
            else
            {
                // Final target reached → hide and stop
                if (dist <= reachRadius)
                {
                    Hide();
                    return;
                }
            }

            if (CurrentTarget == null) { Hide(); return; }

            // ── Screen-space projection ───────────────────────────────────────
            float width  = waypointContainer.layout.width;
            float height = waypointContainer.layout.height;

            float minX = width  / 2f;
            float maxX = Screen.width  - minX;
            float minY = height / 2f;
            float maxY = Screen.height - minY;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(CurrentTarget.position + offset);

            // UI Toolkit Y is flipped vs Camera
            screenPos.y = Screen.height - screenPos.y;

            // Target is behind the player → push icon to the edge
            if (Vector3.Dot(CurrentTarget.position - playerPos, playerFwd) < 0)
                screenPos.x = screenPos.x < Screen.width / 2f ? maxX : minX;

            screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
            screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);

            waypointContainer.style.left = screenPos.x - (width  / 2f);
            waypointContainer.style.top  = screenPos.y - (height / 2f);

            meterLabel.text = Mathf.RoundToInt(dist) + "m";
        }

        private void AdvanceToNextTarget()
        {
            currentIndex++;
            Debug.Log($"[MissionWaypoint] Advanced to target {currentIndex}" +
                      (IsOnFinalTarget ? " (FINAL)" : ""));
        }

        private void OnDrawGizmosSelected()
        {
            if (targets == null) return;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;

                Gizmos.color = (i == targets.Length - 1) ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(targets[i].position, reachRadius);

                if (i < targets.Length - 1 && targets[i + 1] != null)
                    Gizmos.DrawLine(targets[i].position, targets[i + 1].position);
            }
        }
    }
}
