using UnityEngine;
using UnityEngine.UIElements; // Required for UI Toolkit

public class MissionWaypoint : MonoBehaviour
{
    [Header("Target References")]
    public Transform target;
    public Vector3 offset;

    [Header("UI Toolkit")]
    public UIDocument uiDocument; // Assign this in the inspector
    
    // UI Elements queried from the document
    private VisualElement waypointContainer;
    private Label meterLabel;

    private void Start()
    {
        // Query the elements by the names you gave them in UI Builder
        var root = uiDocument.rootVisualElement;
        waypointContainer = root.Q<VisualElement>("WaypointContainer");
        meterLabel = root.Q<Label>("MeterLabel");
    }

    private void Update()
    {
        // Safety check to ensure UI and target exist
        if (waypointContainer == null || target == null) return;

        // Wait for the first frame layout pass to get accurate width/height
        if (float.IsNaN(waypointContainer.layout.width)) return;

        // Get dimensions of the UI element dynamically
        float width = waypointContainer.layout.width;
        float height = waypointContainer.layout.height;

        // Minimum and Maximum bounds based on the element's size
        float minX = width / 2f;
        float maxX = Screen.width - minX;
        float minY = height / 2f;
        float maxY = Screen.height - minY;

        // Get screen position from camera
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);

        // INVERT Y-AXIS: Camera puts 0 at bottom, UI Toolkit puts 0 at top
        screenPos.y = Screen.height - screenPos.y;

        // Check if the target is behind us
        if (Vector3.Dot((target.position - transform.position), transform.forward) < 0)
        {
            // If it's behind and on the left side of the screen, pin it to the right
            if (screenPos.x < Screen.width / 2)
            {
                screenPos.x = maxX;
            }
            else // Pin it to the left
            {
                screenPos.x = minX;
            }
        }

        // Clamp the positions so the icon stays on screen
        screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
        screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);

        // Apply positions. 
        // We subtract half the width/height because 'style.left' and 'style.top' 
        // position the top-left corner of the element, not the center.
        waypointContainer.style.left = screenPos.x - (width / 2f);
        waypointContainer.style.top = screenPos.y - (height / 2f);

        // Update the distance text
        meterLabel.text = Mathf.RoundToInt(Vector3.Distance(target.position, transform.position)).ToString() + "m";
    }
}