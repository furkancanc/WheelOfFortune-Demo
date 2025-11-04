using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Game.Data;

namespace WheelOfFortune.Game.UI
{
    public class RewardPopupAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField, Min(0.05f), Tooltip("Fade-in duration for popup appearance.")]
        private float fadeInDuration = 0.25f;

        [SerializeField, Min(0.05f), Tooltip("Scale-up duration for popup appearance.")]
        private float scaleInDuration = 0.4f;

        [SerializeField, Tooltip("Ease function used for scale-in animation.")]
        private Ease scaleInEase = Ease.OutBack;

        [SerializeField, Min(0.05f), Tooltip("Time popup stays visible before fading out.")]
        private float displayDelay = 0.8f;

        [SerializeField, Min(0.05f), Tooltip("Fade-out duration before fly animation.")]
        private float fadeOutDuration = 0.35f;

        [SerializeField, Tooltip("Ease used for popup fade-out animation.")]
        private Ease fadeOutEase = Ease.InOutSine;

        private RewardPopup _view;
        private CanvasGroup _canvasGroup;
        private Sequence _sequence;

        #region UNITY LIFECYCLE

        private void OnDisable()
        {
            Kill();
        }
        private void OnDestroy()
        {
            Kill();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            fadeInDuration = Mathf.Max(0.05f, fadeInDuration);
            scaleInDuration = Mathf.Max(0.05f, scaleInDuration);
            fadeOutDuration = Mathf.Max(0.05f, fadeOutDuration);
            displayDelay = Mathf.Max(0.05f, displayDelay);
        }
#endif

        #endregion

        #region DEPENDENCY INJECTION
        public void InjectDependency(RewardPopup view)
        {
            _view = view;
            if (_view != null)
            {
                _canvasGroup = _view.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = _view.gameObject.AddComponent<CanvasGroup>();
            }
        }
        #endregion

        #region ANIMATION LOGIC
        public void PlayShowAnimation(Vector3 iconPos,
                                      RewardItem data,
                                      Action<RewardItem, Vector3> onPrepareFly,
                                      Action<RewardItem, Vector3> onExecuteFly,
                                      Action onComplete)
        {
            if (_view == null)
            {
                Debug.LogError($"{nameof(RewardPopupAnimation)}: Missing RewardPopup reference.", this);
                return;
            }

            if (_canvasGroup == null)
                _canvasGroup = _view.GetComponent<CanvasGroup>() ?? _view.gameObject.AddComponent<CanvasGroup>();

            Kill();

            Transform viewTransform = _view.transform;
            _canvasGroup.alpha = 0f;

            _sequence = DOTween.Sequence();

            _sequence.Append(_canvasGroup.DOFade(1f, fadeInDuration))
                     .Join(viewTransform.DOScale(1f, scaleInDuration).SetEase(scaleInEase));

            _sequence.AppendInterval(displayDelay);

            _sequence.AppendCallback(() => onPrepareFly?.Invoke(data, iconPos));

            _sequence.Append(_canvasGroup
                .DOFade(0f, fadeOutDuration)
                .SetEase(fadeOutEase));

            _sequence.AppendCallback(() => onExecuteFly?.Invoke(data, iconPos));

            _sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                _sequence = null;
            });

            _sequence.SetUpdate(true); // TimeScale independent
        }

        #endregion

        #region CLEAN UP
        public void Kill()
        {
            _sequence?.Kill();
            _sequence = null;
        }
        #endregion
    }
}
