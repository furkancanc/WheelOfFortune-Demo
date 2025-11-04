using System;
using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Game.Data;
using WheelOfFortune.Game.UI;
using WheelOfFortune.Infrastructure.Events;
using WheelOfFortune.Infrastructure.Pooling;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.Managers
{
    [System.Serializable]
    public class ZoneVisualMapping
    {
        public ZoneType zoneType;
        public ZoneVisuals zoneVisuals;
    }

    public interface IZoneManager
    {
        int CurrentRound { get; }
        ZoneType CurrentZone { get; }
        void AdvanceRound();
    }

    [DisallowMultipleComponent]
    public sealed class ZoneManager : MonoBehaviour, IZoneManager
    {
        [Header("References")]
        [SerializeField] private ZoneIndicatorView indicatorView;
        [SerializeField] private ZoneConfig zoneConfig;
        [SerializeField] private ZoneCounterView counterPrefab;
        [SerializeField] private RectTransform counterParent;
        [SerializeField] private ZoneCounterAnimation counterAnimation;
        [SerializeField] private ZoneProgressView zoneProgressView;

        [Header("Zone Visuals")]
        [SerializeField] private ZoneVisualsConfig zoneVisualsConfig;

        [Header("Layout Settings")]
        [SerializeField, Min(3)] private int windowSize = 9;
        [SerializeField, Min(16f)] private float cellWidth = 64f;
        [SerializeField, Range(0f, 32f)] private float spacing = 0f;
        [SerializeField, Min(1)] private int maxActiveRounds = 13;

        private PoolManager<ZoneCounterView> _counterPool;
        private readonly List<ZoneCounterView> _activeCounters = new();

        private int _firstShownZoneIndex;
        private int _centerIndex;

        public int CurrentRound { get; private set; } = 1;
        public ZoneType CurrentZone { get; private set; } = ZoneType.Normal;

        #region UNITY LIFECYCLE

        private void Awake()
        {
            InitializeCounterPool();
            UpdateZoneState();
            UpdateProgressUI();
        }

        private void OnEnable()
        {
            Subscribe<RewardCollectedEvent>(OnRewardCollected);
        }

        private void OnDisable()
        {
            Unsubscribe<RewardCollectedEvent>(OnRewardCollected);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoAssignReferences();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        #endregion


        #region INITIALIZATION

        private void InitializeCounterPool()
        {
            if (counterParent == null || counterPrefab == null)
            {
                Debug.LogError($"{nameof(ZoneManager)}: Missing counterParent or counterPrefab.");
                return;
            }

#if UNITY_EDITOR
            foreach (Transform child in counterParent)
                DestroyImmediate(child.gameObject);
#endif
            _counterPool = new PoolManager<ZoneCounterView>(counterPrefab, maxActiveRounds);
            _activeCounters.Clear();

            _centerIndex = Mathf.Max(1, windowSize / 2);

            float totalWidth = windowSize * cellWidth + (windowSize - 1) * spacing;
            counterParent.sizeDelta = new Vector2(totalWidth, counterParent.sizeDelta.y);
            counterParent.pivot = new Vector2(0f, 0.5f);
            counterParent.anchoredPosition = new Vector2(-cellWidth / 2f, 0f);

            for (int i = 0; i < maxActiveRounds; i++)
            {
                var view = _counterPool.Get();
                view.transform.SetParent(counterParent, false);

                RectTransform rt = view.RectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.anchoredPosition = new Vector2(i * (cellWidth + spacing), 0f);

                _activeCounters.Add(view);
            }

            RefreshVisibleWindow(CurrentRound);
        }

        #endregion

        #region CORE LOGIC

        private void OnRewardCollected(RewardCollectedEvent _)
        {
            AdvanceRound();
        }


        public void AdvanceRound()
        {
            ++CurrentRound;
            UpdateZoneState();
            PublishZoneEvents();
            AnimateShift();
            UpdateProgressUI();
        }

        private void UpdateZoneState()
        {
            CurrentZone = DetermineZoneType(CurrentRound);

            if (indicatorView)
                indicatorView.UpdateZone(zoneVisualsConfig.GetBackground(CurrentZone));
            else
                Debug.LogError($"{nameof(ZoneManager)}: Missing ZoneIndicatorView.");
        }

        private void RefreshVisibleWindow(int centerZone)
        {
            _firstShownZoneIndex = Mathf.Max(1, centerZone - _centerIndex);

            for (int i = 0; i < _activeCounters.Count; i++)
            {
                int zoneNumber = _firstShownZoneIndex + i;
                ZoneType type = GetZoneType(zoneNumber);
                bool isCurrent = zoneNumber == CurrentRound;
                bool isPassed = zoneNumber < CurrentRound;

                Color textColor = zoneVisualsConfig.GetTextColor(type, isCurrent, isPassed);
                _activeCounters[i].Setup(zoneNumber, textColor);
            }
        }
        private void UpdateCounterVisuals()
        {
            for (int i = 0; i < _activeCounters.Count; i++)
            {
                int zoneNumber = _firstShownZoneIndex + i;
                ZoneType type = GetZoneType(zoneNumber);
                bool isCurrent = zoneNumber == CurrentRound;
                bool isPassed = zoneNumber < CurrentRound;

                Color textColor = zoneVisualsConfig.GetTextColor(type, isCurrent, isPassed);
                _activeCounters[i].Setup(zoneNumber, textColor);
            }
        }


        private void AnimateShift()
        {
            bool exceededMax = CurrentRound > maxActiveRounds;

            counterAnimation.PlayShiftAnimation(counterParent, cellWidth, spacing, () =>
            {
                if (exceededMax)
                {
                    var firstView = _activeCounters[0];
                    _activeCounters.RemoveAt(0);
                    _counterPool.Return(firstView);
                    _firstShownZoneIndex++;
                }

                int newZoneNumber = _firstShownZoneIndex + _activeCounters.Count;
                var newView = _counterPool.Get();
                newView.transform.SetParent(counterParent, false);
                newView.transform.localScale = Vector3.one;

                RectTransform rt = newView.RectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                float endX = _activeCounters[^1].RectTransform.anchoredPosition.x + cellWidth + spacing;
                rt.anchoredPosition = new Vector2(endX, 0f);

                ZoneType zoneType = GetZoneType(newZoneNumber);
                bool isCurrent = newZoneNumber == CurrentRound;
                bool isPassed = newZoneNumber < CurrentRound;

                Color textColor = zoneVisualsConfig.GetTextColor(zoneType, isCurrent, isPassed);
                newView.Setup(newZoneNumber, textColor);

                _activeCounters.Add(newView);
                UpdateCounterVisuals();
            });
        }

        private void UpdateProgressUI()
        {
            if (zoneProgressView)
                zoneProgressView.UpdateZoneTexts(GetNextSafeZoneRound(), GetNextSuperZoneRound());
        }

        #endregion

        #region ZONE CALCULATION
        private ZoneType GetZoneType(int round) => DetermineZoneType(round);

        private ZoneType DetermineZoneType(int round)
        {
            if (round % zoneConfig.SuperZoneInterval == 0)
                return ZoneType.Super;
            if (round % zoneConfig.SafeZoneInterval == 0)
                return ZoneType.Safe;
            return ZoneType.Normal;
        }

        private int GetNextSafeZoneRound() =>
            ((CurrentRound / zoneConfig.SafeZoneInterval) + 1) * zoneConfig.SafeZoneInterval;

        private int GetNextSuperZoneRound() =>
            ((CurrentRound / zoneConfig.SuperZoneInterval) + 1) * zoneConfig.SuperZoneInterval;

        #endregion

        #region EVENTS

        private void PublishZoneEvents()
        {
            Publish(new RoundAdvancedEvent(CurrentRound));

            switch (CurrentZone)
            {
                case ZoneType.Super:
                    Publish(new SuperZoneUpdatedEvent(GetNextSuperZoneRound()));
                    break;
                case ZoneType.Safe:
                    Publish(new SafeZoneUpdatedEvent(GetNextSafeZoneRound()));
                    break;
            }
        }

        #endregion

#if UNITY_EDITOR
        private void AutoAssignReferences()
        {
            if (indicatorView == null)
                indicatorView = GetComponentInChildren<ZoneIndicatorView>(true);
            if (zoneConfig == null)
                zoneConfig = Resources.Load<ZoneConfig>("ZoneConfig/ZoneConfig");
            if (counterParent == null)
                counterParent = GetComponentInChildren<RectTransform>(true);
            if (counterAnimation == null)
                counterAnimation = GetComponentInChildren<ZoneCounterAnimation>(true);
            if (counterPrefab == null)
                counterPrefab = Resources.Load<ZoneCounterView>("Prefabs/ui_zone-counter-item");
        }
#endif
    }
}
