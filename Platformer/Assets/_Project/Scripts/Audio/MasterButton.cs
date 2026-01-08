using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using FMODUnity;

namespace Platformer
{
    [RequireComponent(typeof(RectTransform))]
    public class MasterButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        
        
     
        
        
        
        [Header("Scale Settings")]
        [Min(0f)]
        [Tooltip("Scale multiplier when selected/hovered.")]
        public float selectedScale = 1.08f;

        [Min(0f)]
        [Tooltip("Tween duration in seconds.")]
        public float animationDuration = 0.15f;

        [Tooltip("DOTween ease for the scale animation.")]
        public Ease ease = Ease.OutBack;

        [Header("Input Modes")]
        [Tooltip("Respond to keyboard/controller selection (ISelect/IDeselect).")]
        public bool respondToSelect = true;

        [Tooltip("Respond to mouse hover (IPointerEnter/Exit).")]
        public bool respondToHover = true;

        private Vector3 baseScale;
        private Tweener scaleTween;
        private bool isSelected;
        private bool isPointerOver;

        private void Awake()
        {
            baseScale = transform.localScale;
        }
        
       
        
       

        private void OnEnable()
        {
            // Ensure visual is consistent on enable
            isSelected = false;
            isPointerOver = false;
            SetScaleImmediate(1f);
        }

        private void OnDisable()
        {
            KillTween();
            // Reset to original to avoid lingering scaled visuals in editor
            transform.localScale = baseScale;
        }
        
        public void PlayOneShot()
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiopen, this.transform.position);
        }
        
        public void PlayOneShotCancel()
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiclose, this.transform.position);
        }

        public void OnSelect(BaseEventData eventData)
        {
            
            if (!respondToSelect) return;
            isSelected = true;
            UpdateVisual();
            // Without checking if the AudioManager and FMODEvent is not null the game will give a error 
            if (AudioManager.instance != null && FMODEvents.instance != null)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.ui, this.transform.position);
            }
        }
        

        public void OnDeselect(BaseEventData eventData)
        {
            if (!respondToSelect) return;
            isSelected = false;
            UpdateVisual();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!respondToHover) return;
            isPointerOver = true;
            UpdateVisual();
            // Without checking if the AudioManager and FMODEvent is not null the game will give a error 
            if (AudioManager.instance != null && FMODEvents.instance != null)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.ui, this.transform.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!respondToHover) return;
            isPointerOver = false;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            float target = ((isSelected && respondToSelect) || (isPointerOver && respondToHover)) ? selectedScale : 1f;
            AnimateScale(target);
        }

        private void AnimateScale(float targetMultiplier)
        {
            KillTween();
            scaleTween = transform
                .DOScale(baseScale * targetMultiplier, animationDuration)
                .SetEase(ease);
        }

        private void SetScaleImmediate(float targetMultiplier)
        {
            KillTween();
            transform.localScale = baseScale * targetMultiplier;
        }

        private void KillTween()
        {
            if (scaleTween != null && scaleTween.IsActive())
            {
                scaleTween.Kill();
                scaleTween = null;
            }
        }
    }
}