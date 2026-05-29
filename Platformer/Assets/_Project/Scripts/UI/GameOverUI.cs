using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Platformer
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;
        [SerializeField] Health playerHealth;
        [SerializeField] float fadeInDuration = 1.5f;

        VisualElement root;

        void Awake()
        {
            root = uiDocument.rootVisualElement.Q<VisualElement>("GameOverRoot");
            root.style.opacity = 0f;
            root.style.display = DisplayStyle.None;
        }

        void OnEnable() => playerHealth.OnDeath += ShowGameOver;
        void OnDisable() => playerHealth.OnDeath -= ShowGameOver;

        void ShowGameOver()
        {
            root.style.display = DisplayStyle.Flex;
            StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                root.style.opacity = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            root.style.opacity = 1f;
        }
    }
}
