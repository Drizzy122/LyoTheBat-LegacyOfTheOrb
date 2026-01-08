using UnityEngine;
using DG.Tweening;

namespace Platformer
{
    public class MenuAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform optionsPanelRect;
        [SerializeField] private float topPosY, middlePosY;
        [SerializeField] private float tweenDuration;

        [SerializeField] private Vector3 startScale = Vector3.one * 0.8f; // Panel starts at 80% of its default size
        [SerializeField] private Ease scaleEase = Ease.OutBack; // Easing function for scaling

        private Vector3 defaultScale;

        private void Awake()
        {
            // Save the default scale so we can restore it
            defaultScale = optionsPanelRect.localScale;

            // Initialize the scale to the starting scale
            optionsPanelRect.localScale = startScale;
        }

        public void OnEnable()
        {
            Intro();
        }

        public void OnDisable()
        {
            DOTween.Kill(optionsPanelRect); // Kill any running tweens
            Outro();

        }

        void Intro()
        {
            if (optionsPanelRect == null) return;
    
            // Tween for position
            optionsPanelRect.DOAnchorPosY(middlePosY, tweenDuration)
                .SetUpdate(true)
                .OnComplete(() => {
                    // Optional: Check if object still exists after tween completes
                    if (optionsPanelRect != null) {
                        // Do something
                    }
                });

            // Tween for scale
            optionsPanelRect.DOScale(defaultScale, tweenDuration)
                .SetEase(scaleEase)
                .SetUpdate(true);

        }

        void Outro()
        {
            // Tween for position
            optionsPanelRect.DOAnchorPosY(topPosY, tweenDuration).SetUpdate(true);

            // Tween for returning to startScale
            optionsPanelRect.DOScale(startScale, tweenDuration).SetEase(scaleEase).SetUpdate(true);
        }

        private void OnDestroy()
        {
            // Ensure all tweens for this RectTransform are killed
            DOTween.Kill(optionsPanelRect);
        }
    }
}