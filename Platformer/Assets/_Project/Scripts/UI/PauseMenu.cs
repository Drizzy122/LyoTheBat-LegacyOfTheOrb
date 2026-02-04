using UnityEngine;
using KBCore.Refs;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Platformer
{
    public class PauseMenu : ValidatedMonoBehaviour
    {
        [field: Header("Configs")] 
        [field: SerializeField, Anywhere] InputReader input;
        [field: SerializeField] GameObject pauseUI;
        [field: SerializeField, Anywhere] PlayerController playerController;
        public Button primaryButton;
        [field: SerializeField] bool isPaused = false;
        [field: SerializeField] string musicName;
        [field: SerializeField] float musicValue = 1f; 
        private float pausedValue = 0f;
        void Start()
        {
            if (!isPaused)
            {
                Time.timeScale = 1;
                pauseUI.SetActive(false);
                isPaused = false;
                AudioManager.instance.SetMusicParameter(musicName, musicValue);
                AudioManager.instance.SetAmbienceParameter(musicName, musicValue);
            }
            SelectButton();
        }
        private void SelectButton()
        {
            if (primaryButton != null)
            {
                EventSystem.current.SetSelectedGameObject(primaryButton.gameObject);
            }
        }

        private void OnPause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                ActivateMenu();
            }
            else
            {
                DeactivateMenu();
            }
        }
        void ActivateMenu()
        {
            Time.timeScale = 0;
            pauseUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // Disable the PlayerController script
            if (playerController != null) playerController.enabled = false;
            AudioManager.instance.SetMusicParameter(musicName, pausedValue);
            AudioManager.instance.SetAmbienceParameter(musicName, pausedValue);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiopen, this.transform.position);

        }
        public void DeactivateMenu()
        {
            Time.timeScale = 1;
            pauseUI.SetActive(false);
            isPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Re-enable the PlayerController script
            if (playerController != null) playerController.enabled = true;
            AudioManager.instance.SetMusicParameter(musicName, musicValue);
            AudioManager.instance.SetAmbienceParameter(musicName, musicValue);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiclose, this.transform.position);
        }
        
        public void QuitGame()
        {
            Debug.Log("Quitting Game");
            #if UNITY_EDITOR
            // Stop Play Mode in the Editor
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            // Close the application in a build
            Application.Quit();
            #endif
        }
        private void OnEnable() => input.Paused += OnPause ;
        private void OnDisable() => input.Paused -= OnPause;
    }
}