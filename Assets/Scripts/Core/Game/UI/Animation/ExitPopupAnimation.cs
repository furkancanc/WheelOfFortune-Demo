using DG.Tweening;
using System;
using UnityEngine;

namespace WheelOfFortune.Game.UI
{
    public class ExitPopupAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField, Range(0.1f, 1f), Tooltip("Duration for fade-in animation.")]
        private float fadeInDuration = 0.3f;

        [SerializeField, Range(0.1f, 1f), Tooltip("Duration for fade-out animation.")]
        private float fadeOutDuration = 0.25f;

        [SerializeField, Range(0.5f, 1f), Tooltip("Starting scale for popup when shown.")]
        private float startScale = 0.8f;

        [SerializeField, Tooltip("Ease used for show animation scale.")]
        private Ease showEase = Ease.OutBack;

        private CanvasGroup _canvasGroup;
        private Transform _transform;
        private Sequence _sequence;

        #region UNITY LIFECYCLE
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-assign CanvasGroup from children if missing
            if (!_canvasGroup)
                _canvasGroup = GetComponentInChildren<CanvasGroup>(true);

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        private void OnDisable() => KillActiveTweens();
        private void OnDestroy() => KillActiveTweens();

        #endregion



        #region DEPENDENCY INJECTION
        public void InjectDependency(CanvasGroup canvasGroup, Transform transform)
        {
            _canvasGroup = canvasGroup;
            _transform = transform;
        }

        #endregion

        #region ANIMATIONS
        public void PlayShow()
        {
            if (!ValidateDependencies()) return;

            KillActiveTweens();

            _canvasGroup.alpha = 0f;
            _transform.localScale = Vector3.one * startScale;

            _sequence = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, fadeInDuration))
                .Join(_transform.DOScale(1f, fadeInDuration + 0.05f).SetEase(showEase))
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() => _sequence = null);
        }

        public void PlayHide(Action onComplete)
        {
            if (!ValidateDependencies())
            {
                onComplete?.Invoke();
                return;
            }

            KillActiveTweens();

            _sequence = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(0f, fadeOutDuration))
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .SetLink(gameObject)
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
        private bool ValidateDependencies()
        {
            if (_canvasGroup == null || _transform == null)
            {
                Debug.LogError($"{nameof(ExitPopupAnimation)}: Missing references. Animation skipped.", this);
                return false;
            }
            return true;
        }
        #endregion

    }
}
