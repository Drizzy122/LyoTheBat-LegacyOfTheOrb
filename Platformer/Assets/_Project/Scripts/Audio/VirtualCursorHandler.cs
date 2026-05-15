using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.UI;

public class UIToolkitCursorSync : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VirtualMouseInput virtualMouseInput;
    
    private VisualElement cursorElement;

    void OnEnable()
    {
        // Grab the cursor we made in UI Builder
        cursorElement = uiDocument.rootVisualElement.Q<VisualElement>("VirtualCursor");
    }

    
    
    void Update()
    {
        if (cursorElement == null || virtualMouseInput == null) return;

        // 1. Get the virtual mouse position from the component
        Vector2 mousePos = virtualMouseInput.virtualMouse.position.ReadValue();

        // 2. Convert screen coordinates to UI Toolkit coordinates
        // UI Toolkit (0,0) is top-left. Screen (0,0) is bottom-left.
        float uiX = mousePos.x;
        float uiY = Screen.height - mousePos.y;

        // 3. Update the UI element position
        cursorElement.style.left = uiX;
        cursorElement.style.top = uiY;
    }
}