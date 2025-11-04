using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using WheelOfFortune.Game.Data;
using Image = UnityEngine.UI.Image;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class WheelView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image wheelImage;
        [SerializeField] private Image indicatorImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI infoText;

        [Header("Wheel Slice References")]
        [SerializeField] private Transform sliceParent;
        [SerializeField] private WheelSlice slicePrefab;

        [Header("Wheel Animation")]
        [SerializeField] private WheelAnimation wheelAnimation;

        [Header("Layout Settings")]
        [Range(0f, 360f)][SerializeField] private float radius = 180f;
        [Range(0f, 360f)][SerializeField] private float startAngleDeg = 90f;


        private readonly List<WheelSlice> _activeSlices = new();
        public WheelAnimation Animation => wheelAnimation;

        #region Slice Layout
        public void DisplaySlices(RewardItem[] sliceDataArray)
        {
            if (sliceDataArray == null || sliceDataArray.Length == 0)
                return;

            int count = sliceDataArray.Length;
            EnsureSliceCount(count);

            float step = 360f / count;
            Vector2 offset = Vector2.zero;

            for (int i = 0; i < count; i++)
            {
                WheelSlice slice = _activeSlices[i];
                RectTransform rt = slice.GetComponent<RectTransform>();

                float angle = startAngleDeg + step * i;
                float rad = angle * Mathf.Deg2Rad;

                offset.Set(Mathf.Cos(rad), Mathf.Sin(rad));
                rt.localPosition = Vector3.zero;
                rt.localScale = Vector3.one;
                rt.anchoredPosition = offset * radius;
                rt.localRotation = Quaternion.Euler(0, 0, angle - 90f);

                slice.Setup(sliceDataArray[i]);
            }
        }

        private void EnsureSliceCount(int needed)
        {
            while (_activeSlices.Count < needed)
            {
                var slice = Instantiate(slicePrefab, sliceParent);
                _activeSlices.Add(slice);
            }

            for (int i = 0; i < _activeSlices.Count; i++)
                _activeSlices[i].gameObject.SetActive(i < needed);
        }
        #endregion

        #region Animation Controls
        public void AnimateSpin(float duration, float targetAngle, TweenCallback onComplete)
        {
            wheelAnimation?.AnimateSpin(duration, targetAngle, onComplete);
        }

        public void ResetWheel(bool animate = true)
        {
            wheelAnimation?.ResetWheel(animate);
        }

        public void HighlightIndicator()
        {
            wheelAnimation?.HighlightIndicator();
        }
        #endregion


        #region UI Updates
        public void UpdateWheelView(WheelConfig config)
        {
            if (config == null)
            {
                Debug.LogError($"{nameof(WheelView)}: Missing WheelConfig data.", this);
                return;
            }

            if (wheelImage != null && config.WheelSprite != null)
                wheelImage.sprite = config.WheelSprite;

            if (indicatorImage != null && config.IndicatorSprite != null)
                indicatorImage.sprite = config.IndicatorSprite;

            if (titleText != null)
            {
                titleText.SetText(config.Title);
                titleText.color = config.TextColor;
            }

            if (infoText != null)
            {
                infoText.SetText(config.Info);
                infoText.color = config.TextColor;
            }
        }
        #endregion

#if UNITY_EDITOR

        private void OnValidate()
        {
            TryAutoAssign(ref wheelImage, "Root_Image");
            TryAutoAssign(ref indicatorImage, "Indicator");
            TryAutoAssign(ref titleText, "Title");
            TryAutoAssign(ref infoText, "Info");
        }
        private static void TryAutoAssign<T>(ref T target, string nameHint) where T : Object
        {
            if (target != null) return;

            var foundObjects = Resources.FindObjectsOfTypeAll<T>();
            foreach (var obj in foundObjects)
            {
                if (obj.name.ToLower().Contains(nameHint.ToLower()))
                {
                    target = obj;
                    return;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!sliceParent) return;

            const int sampleCount = 8;
            float step = 360f / sampleCount;
            Vector3 center = sliceParent.position;

            Gizmos.color = Color.yellow;

            for (int i = 0; i < sampleCount; i++)
            {
                float angle = startAngleDeg + step * i;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 localPos = new(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector3 worldPos = sliceParent.TransformPoint(localPos * radius);

                Handles.Label(worldPos + Vector3.up * 15f, $"Slice {i}");
                Gizmos.DrawWireSphere(worldPos, 10f);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center, radius);
        }
#endif
    }
}
