using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Game.Data;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class RewardPopup : MonoBehaviour
    {
        [Header("UI References (Auto-assigned)")]
        [SerializeField, Tooltip("Reward icon image.")]
        private Image iconImage;

        [SerializeField, Tooltip("Text displaying reward count.")]
        private TextMeshProUGUI countText;

        [SerializeField, Tooltip("Text displaying reward name.")]
        private TextMeshProUGUI nameText;

        [SerializeField, Tooltip("CanvasGroup controlling fade and visibility.")]
        private CanvasGroup canvasGroup;

        [SerializeField, Tooltip("Animation handler controlling reward popup visuals.")]
        private RewardPopupAnimation animationHandler;

        private RectTransform _rectTransform;
        private RewardItem _rewardData;
        private Action<RewardItem, Vector3> _onPrepareFly;
        private Action<RewardItem, Vector3> _onExecuteFly;

        public RectTransform IconRectTransform => iconImage ? iconImage.rectTransform : null;

        #region UNITY LIFECYCLE
#if UNITY_EDITOR
        private void OnValidate()
        {
            TryAutoAssign(ref iconImage, "Icon");
            TryAutoAssign(ref countText, "Count");
            TryAutoAssign(ref nameText, "Name");
            TryAutoAssign(ref animationHandler, "Animation");

            if (!canvasGroup)
            {
                canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }

        private static void TryAutoAssign<T>(ref T target, string nameHint) where T : Component
        {
            if (target != null) return;

            var found = Resources.FindObjectsOfTypeAll<T>()
                .FirstOrDefault(c => c.name.ToLower().Contains(nameHint.ToLower()));

            if (found != null)
                target = found;
        }
#endif
        private void Awake()
        {
            _rectTransform = transform as RectTransform;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            if (animationHandler == null)
                Debug.LogWarning($"{nameof(RewardPopup)}: Missing animation handler reference.", this);
        }

        private void OnDisable()
        {
            animationHandler?.Kill();
        }

        #endregion

        #region PUBLIC API

        public void Show(RewardItem data,
                         Action<RewardItem, Vector3> onPrepareFly,
                         Action<RewardItem, Vector3> onExecuteFly,
                         Action onComplete)
        {
            if (data == null)
            {
                Debug.LogError($"{nameof(RewardPopup)}: Reward data is null.", this);
                return;
            }

            _rewardData = data;
            _onPrepareFly = onPrepareFly;
            _onExecuteFly = onExecuteFly;

            if (iconImage) iconImage.sprite = data.Icon;
            if (nameText) nameText.SetText(data.DisplayName);
            if (countText) countText.SetText($"x{data.Count}");

            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one * 0.8f;
            gameObject.SetActive(true);

            Vector3 iconPos = iconImage ? iconImage.rectTransform.position : transform.position;

            animationHandler?.InjectDependency(this);

            animationHandler?.PlayShowAnimation(
                iconPos,
                _rewardData,
                _onPrepareFly,
                _onExecuteFly,
                () =>
                {
                    transform.localScale = Vector3.one * 0.8f;
                    onComplete?.Invoke();
                    gameObject.SetActive(false);
                });
        }
        #endregion
    }
}

