using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Platformer
{
    [RequireComponent(typeof(UIDocument))]
    public class ConfirmationPopUpMenu : MonoBehaviour
    {
        private UIDocument document;
        private VisualElement rootContainer;
        
        private Label displayText;
        private Button confirmButton;
        private Button cancelButton;

        // We need to store these so we can unsubscribe from them later to prevent memory leaks/double-clicks
        private Action currentConfirmAction;
        private Action currentCancelAction;

        private void Awake()
        {
            document = GetComponent<UIDocument>();
            rootContainer = document.rootVisualElement.Q<VisualElement>("ConfirmationPopupMenuContainer");

            displayText = rootContainer.Q<Label>("DisplayText");
            confirmButton = rootContainer.Q<Button>("ConfirmButton");
            cancelButton = rootContainer.Q<Button>("CancelButton");

            DeactivateMenu();
        }

        private void Start() {
            AudioManager.instance.RegisterButtonAudio(confirmButton);
            AudioManager.instance.RegisterButtonAudio(cancelButton, isCloseAction: true);
        }

        public void ActivateMenu(string text, Action confirmAction, Action cancelAction)
        {
            rootContainer.style.display = DisplayStyle.Flex;
            this.displayText.text = text;

            // Unsubscribe from old events if they exist
            if (currentConfirmAction != null) confirmButton.clicked -= currentConfirmAction;
            if (currentCancelAction != null) cancelButton.clicked -= currentCancelAction;

            // Assign new actions that include closing the menu
            currentConfirmAction = () => {
                DeactivateMenu();
                confirmAction();
            };
            
            currentCancelAction = () => {
                DeactivateMenu();
                cancelAction();
            };

            // Subscribe to the buttons
            confirmButton.clicked += currentConfirmAction;
            cancelButton.clicked += currentCancelAction;
            
            // Focus the cancel button by default for safety
            cancelButton.Focus();
        }

        // Change this from private to public!
        public void DeactivateMenu() 
        {
            rootContainer.style.display = DisplayStyle.None;
        }
    }
}