using System;
using DG.Tweening;
using UnityEngine;

namespace WheelOfFortune.Game.UI
{ 
    [DisallowMultipleComponent]
    public class BombPopupAnimation : MonoBehaviour
    {
        [Header("UI Elements (Auto-assigned)")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField, Range(0.1f, 1f)] private float showDuration = 0.35f;
        [SerializeField, Range(0.1f, 1f)] private float hideDuration = 0.25f;
        [SerializeField] private Vector3 startScale = new(0.8f, 0.8f, 0.8f);
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;

        private BombPopupView _view;
        private RectTransform _rectTransform;
        private Sequence _sequence;

        #region UNITY LIFECYCLE

        private void OnDisable()
        {
            KillActiveTweens();
        }

        private void OnDestroy()
        {
            KillActiveTweens();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!canvasGroup)
                canvasGroup = GetComponentInChildren<CanvasGroup>(true);

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion

        #region DEPENDENCY INJECTION
        public void InjectDependency(BombPopupView view)
        {
            _view = view;

            _rectTransform = view.GetComponent<RectTransform>();
            if (_rectTransform == null)
                Debug.LogWarning($"{nameof(BombPopupAnimation)}: Missing RectTransform reference.", this);
        }
        #endregion

        #region ANIMATIONS
        public void PlayShowAnimation()
        {
            if (!ValidateComponents()) return;

            KillActiveTweens();

            _rectTransform.localScale = startScale;
            canvasGroup.alpha = 0f;

            _sequence = DOTween.Sequence()
                .Append(canvasGroup.DOFade(1f, showDuration * 0.8f))
                .Join(_rectTransform.DOScale(Vector3.one, showDuration).SetEase(showEase))
                .SetUpdate(true)
                .OnComplete(() => _sequence = null);
        }

        public void PlayHideAnimation(Action onComplete)
        {
            if (!ValidateComponents()) return;

            KillActiveTweens();

            _sequence = DOTween.Sequence()
                .Append(canvasGroup.DOFade(0f, hideDuration))
                .Join(_rectTransform.DOScale(startScale, hideDuration).SetEase(hideEase))
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    _sequence = null;
                });
        }

        #endregion

        #region CLEANUP
        public void KillActiveTweens()
        {
            _sequence?.Kill();
            _sequence = null;
        }
        #endregion

        #region UTILITIES
        private bool ValidateComponents()
        {
            if (_rectTransform == null || canvasGroup == null)
            {
                Debug.LogWarning($"{nameof(BombPopupAnimation)}: Missing references — animation skipped.", this);
                return false;
            }
            return true;
        }

        #endregion  

    }
}
