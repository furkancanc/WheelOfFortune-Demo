using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Game.Data;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class ZoneCounterAnimation : MonoBehaviour
    {
        [Header("Tween Settings")]
        [SerializeField, Min(0.05f), Tooltip("Duration of the horizontal shift animation.")]
        private float shiftDuration = 0.25f;

        [SerializeField, Tooltip("Easing used for the shift animation.")]
        private Ease ease = Ease.OutQuart;

        private Tween _shiftTween;
        private Tween _pulseTween;

        #region UNITY LIFECYCLE
        private void OnDisable()
        {
            KillActiveTweens();
        }
        #endregion

        #region SHIFT ANIMATION
        public void PlayShiftAnimation(RectTransform content, float cellWidth, float spacing, Action onComplete)
        {
            if (content == null)
                return;

            float targetDeltaX = -(cellWidth + spacing);

            _shiftTween?.Kill();
            content.DOKill();

            _shiftTween = content
                .DOAnchorPosX(content.anchoredPosition.x + targetDeltaX, shiftDuration)
                .SetEase(ease)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    _shiftTween = null;
                });
        }
        #endregion

        #region PULSE ANIMATION
        public void PlayScalePulse(RectTransform target)
        {
            if (target == null)
                return;

            _pulseTween?.Kill();
            target.DOKill();

            _pulseTween = target
                .DOScale(1.15f, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (target != null)
                        target.localScale = Vector3.one;
                    _pulseTween = null;
                });
        }
        #endregion  

        #region CLEANUP
        public void KillActiveTweens()
        {
            _shiftTween?.Kill();
            _pulseTween?.Kill();
            _shiftTween = null;
            _pulseTween = null;
        }
        #endregion
    }
}
