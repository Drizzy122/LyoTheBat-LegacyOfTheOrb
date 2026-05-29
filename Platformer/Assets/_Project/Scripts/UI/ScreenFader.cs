using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Platformer
{
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;
        [SerializeField] float fadeDuration = 1f;

        VisualElement overlay;

        void Start()
        {
            overlay = uiDocument.rootVisualElement.Q<VisualElement>("FadeOverlay");
            StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                overlay.style.opacity = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            overlay.style.opacity = 0f;
            overlay.style.display = DisplayStyle.None;
        }
    }
}
