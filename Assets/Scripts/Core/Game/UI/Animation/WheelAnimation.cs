using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class WheelAnimation : MonoBehaviour
    {
        [Header("References (Auto-assigned)")]
        [SerializeField, Tooltip("Transform of the wheel object.")]
        private Transform wheelTransform;

        [SerializeField, Tooltip("Transform of the indicator object.")]
        private Transform indicatorTransform;

        [Header("Animation Settings")]
        [SerializeField, Min(0.1f), Tooltip("Extra overshoot angle at the end of spin.")]
        private float overshootAngle = 12f;

        [SerializeField, Min(0.05f), Tooltip("Time it takes to settle after overshoot.")]
        private float settleDuration = 0.4f;

        [SerializeField, Tooltip("Ease used for wheel settle animation.")]
        private Ease settleEase = Ease.OutBack;

        [SerializeField, Tooltip("Ease used for the main wheel spin.")]
        private Ease spinEase = Ease.OutQuart;

        [SerializeField, Min(0.05f), Tooltip("Time to reset wheel back to zero rotation.")]
        private float resetDuration = 0.1f;

        private Sequence _spinSequence;
        private Sequence _indicatorSequence;

        #region UNITY LIFECYCLE

        private void OnDisable()
        {
            KillActiveTweens();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!wheelTransform)
                wheelTransform = FindInParentChildren(transform, "root_wheel");

            if (!indicatorTransform)
                indicatorTransform = FindInParentChildren(transform, "root_indicator");
            if (!wheelTransform)
                Debug.LogWarning($"{nameof(WheelAnimation)}: Could not find 'Wheel' under parent hierarchy.", this);

            if (!indicatorTransform)
                Debug.LogWarning($"{nameof(WheelAnimation)}: Could not find 'Indicator' under parent hierarchy.", this);

            UnityEditor.EditorUtility.SetDirty(this);
        }
        private static Transform FindInParentChildren(Transform self, string nameHint)
        {
            var parent = self.parent;
            if (parent == null)
                return null;

            foreach (Transform sibling in parent)
            {
                if (sibling.name.ToLower().Contains(nameHint.ToLower()))
                    return sibling;

                foreach (Transform nested in sibling)
                {
                    if (nested.name.ToLower().Contains(nameHint.ToLower()))
                        return nested;
                }
            }

            return null;
        }
#endif
        #endregion


        #region Public API
        public void AnimateSpin(float duration, float targetAngle, TweenCallback onComplete)
        {
            if (wheelTransform == null)
            {
                Debug.LogError($"{nameof(WheelAnimation)}: Missing wheelTransform reference.", this);
                return;
            }

            KillActiveTweens();

            float overshoot = Random.Range(overshootAngle * 0.8f, overshootAngle * 1.2f);
            float settleTime = Random.Range(settleDuration * 0.8f, settleDuration * 1.2f);

            _spinSequence = DOTween.Sequence()
                .Append(wheelTransform
                    .DORotate(new Vector3(0, 0, targetAngle + overshoot), duration, RotateMode.FastBeyond360)
                    .SetEase(spinEase))
                .Append(wheelTransform
                    .DORotate(new Vector3(0, 0, targetAngle), settleTime)
                    .SetEase(settleEase))
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    _spinSequence = null;
                });
        }

        public void ResetWheel(bool animate = true)
        {
            KillActiveTweens();

            if (wheelTransform == null)
                return;

            if (animate)
            {
                wheelTransform
                    .DORotate(Vector3.zero, resetDuration)
                    .SetEase(Ease.InOutQuad)
                    .SetUpdate(true)
                    .OnComplete(HighlightIndicator);
            }
            else
            {
                wheelTransform.localRotation = Quaternion.identity;
                HighlightIndicator();
            }
        }

        public void HighlightIndicator()
        {
            if (indicatorTransform == null)
                return;

            _indicatorSequence?.Kill();
            indicatorTransform.DOKill();

            _indicatorSequence = DOTween.Sequence()
                .Append(indicatorTransform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack))
                .Append(indicatorTransform.DOScale(1f, 0.3f).SetEase(Ease.InBack))
                .Join(indicatorTransform.DOShakePosition(0.4f, new Vector3(8, 8, 0), vibrato: 10))
                .SetEase(Ease.OutExpo)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (indicatorTransform != null)
                        indicatorTransform.localScale = Vector3.one;
                    _indicatorSequence = null;
                });
        }

        public void KillActiveTweens()
        {
            _spinSequence?.Kill();
            _indicatorSequence?.Kill();
            _spinSequence = null;
            _indicatorSequence = null;
        }
        #endregion
    }
}
