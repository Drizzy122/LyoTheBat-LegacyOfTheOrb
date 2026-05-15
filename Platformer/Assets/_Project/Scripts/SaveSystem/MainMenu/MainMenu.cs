using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
namespace Platformer
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        [Header("Menu Navigation")]
        [SerializeField] private SaveSlotsMenu saveSlotsMenu;
        [SerializeField] private SettingsManager settingsMenu;
        

        private UIDocument document;
        private VisualElement rootContainer;

        private Button newGameButton;
        private Button continueGameButton;
        private Button loadGameButton;
        private Button settingsButton;

        private void Awake() 
        {
            document = GetComponent<UIDocument>();
            rootContainer = document.rootVisualElement;

            // Query buttons
            newGameButton = rootContainer.Q<Button>("NewGameButton");
            continueGameButton = rootContainer.Q<Button>("ContinueGameButton");
            loadGameButton = rootContainer.Q<Button>("LoadGameButton");
            settingsButton = rootContainer.Q<Button>("SettingsButton");

            // Bind clicks
            newGameButton.clicked += OnNewGameClicked;
            continueGameButton.clicked += OnContinueGameClicked;
            loadGameButton.clicked += OnLoadGameClicked;
            settingsButton.clicked += OnSettingsClicked; // Add this when you make a settings menu!
        }

        private void Start() 
        {
            DisableButtonsDependingOnData();
            SetInitialFocus();
        }

        private void DisableButtonsDependingOnData() 
        {
            if (!DataPersistenceManager.instance.HasGameData()) 
            {
                continueGameButton.SetEnabled(false);
                loadGameButton.SetEnabled(false);
            }
        }

        public void OnNewGameClicked() 
        {
            saveSlotsMenu.ActivateMenu(false, () => this.ActivateMenu());
            this.DeactivateMenu();
        }

        public void OnLoadGameClicked() 
        {
            saveSlotsMenu.ActivateMenu(true, () => this.ActivateMenu());
            this.DeactivateMenu();
        }

        public void OnSettingsClicked()
        {
            // We must pass the instruction "() => this.ActivateMenu()" into the settings menu!
            settingsMenu.ActivateMenu(() => this.ActivateMenu()); 
            this.DeactivateMenu();
        }

        public void OnContinueGameClicked() 
        {
            DisableMenuButtons();
            DataPersistenceManager.instance.SaveGame();
            SceneManager.LoadSceneAsync("Game");
        }

        private void DisableMenuButtons() 
        {
            newGameButton.SetEnabled(false);
            continueGameButton.SetEnabled(false);
        }

        public void ActivateMenu() 
        {
            rootContainer.style.display = DisplayStyle.Flex;
            DisableButtonsDependingOnData();
            SetInitialFocus();
        }

        public void DeactivateMenu() 
        {
            rootContainer.style.display = DisplayStyle.None;
        }
        
        private void SetInitialFocus()
        {
            StartCoroutine(FocusAfterDelay());
        }
        
        private IEnumerator FocusAfterDelay()
        {
            // Wait exactly one frame so UI Toolkit can finish building the menu
            yield return null;

            // If the Continue button is active, focus it first (matching your comment!)
            if (continueGameButton.enabledSelf) 
            {
                continueGameButton.Focus();
            }
            // Otherwise, focus the New Game button
            else if (newGameButton.enabledSelf) 
            {
                newGameButton.Focus();
            }
        }
    }
}