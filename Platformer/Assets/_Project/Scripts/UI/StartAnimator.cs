using DG.Tweening;
using UnityEngine;

namespace Platformer
{

    public class StartAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform optionsPanelRect;
        [SerializeField] private Vector3 startScale = Vector3.zero; // The starting small scale
        [SerializeField] private float scaleDuration = 0.5f; // How long the scaling animation will take
        [SerializeField] private Ease scaleEase = Ease.OutBack; // Animation easing type

        private Vector3 defaultScale;

        private void Awake()
        {
            // Store the default scale to scale back to it later
            defaultScale = optionsPanelRect.localScale;

            // Initialize the panel scale to start small
            optionsPanelRect.localScale = startScale;
        }

        private void OnEnable()
        {
            Intro();
        }

        private void OnDisable()
        {
            Outro();
        }

        private void Intro()
        {
            // Scale the panel gradually to its default size
            optionsPanelRect.DOScale(defaultScale, scaleDuration).SetEase(scaleEase).SetUpdate(true);
        }

        private void Outro()
        {
            // Optionally scale it back to the starting scale when disabling it
            optionsPanelRect.DOScale(startScale, scaleDuration).SetEase(scaleEase).SetUpdate(true);
        }

        private void OnDestroy()
        {
            // Ensure all tweens for this RectTransform are killed
            DOTween.Kill(optionsPanelRect);
        }
    }
}