using UnityEngine;
using DG.Tweening;

namespace Platformer
{
    public class SaveButtonAnimator : MonoBehaviour
    {
        [SerializeField] RectTransform buttonRect;
        [SerializeField] float sidePosX, middlePosY;
        [SerializeField] float tweenDuration;
        
        public void OnEnable()
        {
            Intro();
        }

        public void OnDisable()
        {
           
            Outro();
        }
        
        void Intro()
        {
            buttonRect.DOAnchorPosX(middlePosY, tweenDuration).SetUpdate(true);
        }

        
        void Outro()
        {
            buttonRect.DOAnchorPosX(sidePosX, tweenDuration).SetUpdate(true);
        }
        
        private void OnDestroy()
        {
            DOTween.Kill(buttonRect); // Ensure all tweens for this RectTransform are killed
        }
    }
}