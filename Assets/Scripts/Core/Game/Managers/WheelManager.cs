using System;
using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Game.Data;
using WheelOfFortune.Game.Systems;
using WheelOfFortune.Game.UI;
using WheelOfFortune.Infrastructure.Events;
using WheelOfFortune.Infrastructure.Pooling;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.Managers
{
    [System.Serializable]
    public class WheelConfigEntry
    {
        public WheelType type;
        public WheelConfig config;
    }

    public interface IWheelManager
    {
        void Initialize();
        void Spin();
        bool IsSpinning { get; }
        void ResetWheel();
    }

    [DisallowMultipleComponent]
    public class WheelManager : MonoBehaviour, IWheelManager
    {
        [Header("References")]
        [SerializeField] private MonoBehaviour gameStateProviderBehaviour;

        [Header("References (Auto-assigned)")]
        [SerializeField] private WheelView wheelView;
        [SerializeField] private WheelRoundData wheelRoundData;
        [SerializeField] private WheelSlice slicePrefab;

        [Header("Configurations")]
        [Tooltip("Wheel configurations for different wheel types.")]
        [SerializeField] private List<WheelConfigEntry> wheelConfigsList = new();

        [Header("Spin Settings")]
        [SerializeField, Min(1)] private int spinRotations = 6;
        [SerializeField, Range(0.1f, 10f)] private float minDuration = 2.5f;
        [SerializeField, Range(0.1f, 10f)] private float maxDuration = 3.5f;

        private readonly Dictionary<WheelType, WheelConfig> _wheelConfigs = new();
        private PoolManager<WheelSlice> _slicePool;
        private IWheelStrategy _strategy;
        private IGameStateProvider _gameStateProvider;

        private RewardItem[] _currentSlices;
        private int _sliceCount;
        private WheelConfig _currentConfig;
        public bool IsSpinning { get; private set; }

        #region UNITY LIFECYCLE
        private void Awake()
        {
            ResolveDependencies();
            InitializePools();
            InitializeConfigs();
            Initialize();
        }

        private void OnEnable()
        {
            Subscribe<RoundAdvancedEvent>(OnRoundAdvanced);
            Subscribe<WheelSpinStartRequested>(OnSpinStartRequested);
            Subscribe<SafeZoneUpdatedEvent>(OnSafeZoneUpdated);
            Subscribe<SuperZoneUpdatedEvent>(OnSuperZoneUpdated);
            Subscribe<ReviveButtonClicked>(OnReviveButtonClicked);
        }

        private void OnDisable()
        {
            Unsubscribe<RoundAdvancedEvent>(OnRoundAdvanced);
            Unsubscribe<WheelSpinStartRequested>(OnSpinStartRequested);
            Unsubscribe<SafeZoneUpdatedEvent>(OnSafeZoneUpdated);
            Unsubscribe<SuperZoneUpdatedEvent>(OnSuperZoneUpdated);
            Subscribe<ReviveButtonClicked>(OnReviveButtonClicked);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!wheelView)
                wheelView = FindFirstObjectByType<WheelView>();
            if (!slicePrefab)
                slicePrefab = Resources.Load<WheelSlice>("Prefabs/ui_wheel-slice");
            if (!wheelRoundData)
                wheelRoundData = Resources.Load<WheelRoundData>("Data/WheelData/WheelRoundsData");
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion


        #region INITIALIZATION
        public void Initialize()
        {
            if (wheelRoundData == null || wheelView == null)
            {
                Debug.LogError("[WheelManager] Missing essential references.");
                return;
            }

            wheelRoundData.InitializeCache();
            SetStrategy(WheelType.Bronze);
            SetWheelConfig(WheelType.Bronze);

            LoadRoundSlices(1);
        }

        private void InitializePools()
        {
            if (slicePrefab == null)
            {
                Debug.LogError("[WheelManager] Missing WheelSlice prefab.");
                return;
            }
            _slicePool = new PoolManager<WheelSlice>(slicePrefab, 8);
        }

        private void InitializeConfigs()
        {
            _wheelConfigs.Clear();
            foreach (var entry in wheelConfigsList)
            {
                if (entry != null && !_wheelConfigs.ContainsKey(entry.type))
                    _wheelConfigs.Add(entry.type, entry.config);
            }
        }

        private void ResolveDependencies()
        {
            if (gameStateProviderBehaviour is IGameStateProvider provider)
                _gameStateProvider = provider;
            else
                _gameStateProvider = FindAnyObjectByType<GameStateProvider>(FindObjectsInactive.Include);

            if (_gameStateProvider == null)
                Debug.LogError("[WheelManager] Missing GameStateProvider reference!");
        }
        #endregion

        #region EVENTS

        private void OnReviveButtonClicked(ReviveButtonClicked clicked)
        {
            ResetWheel();
        }

        private void OnSpinStartRequested(WheelSpinStartRequested e)
        {
            if (!IsSpinning)
            {
                IsSpinning = true;
                Spin();
            }
        }

        private void OnRoundAdvanced(RoundAdvancedEvent e)
        {
            if (e == null) return;
            var round = e.CurrentRound;

            if (!_gameStateProvider.IsSafeZone && !_gameStateProvider.IsSuperZone)
                SetWheelConfig(WheelType.Bronze);

            LoadRoundSlices(round);
        }

        private void OnSafeZoneUpdated(SafeZoneUpdatedEvent e)
        {
            SetWheelConfig(WheelType.Silver);
        }

        private void OnSuperZoneUpdated(SuperZoneUpdatedEvent e)
        {
            SetWheelConfig(WheelType.Gold);
        }
        #endregion


        #region WHEEL LOGIC
        private void LoadRoundSlices(int round)
        {
            if (wheelRoundData == null)
            {
                Debug.LogError("[WheelManager] WheelRoundData reference is missing.");
                return;
            }

            _currentSlices = wheelRoundData.GetSlicesForRound(round);
            _sliceCount = _currentSlices?.Length ?? 0;

            if (_sliceCount == 0)
            {
                Debug.LogError($"[WheelManager] No slice data for round {round}.");
                return;
            }

            wheelView.DisplaySlices(_currentSlices);
        }

        public void Spin()
        {
            if (_strategy == null)
            {
                Debug.LogError("[WheelManager] No spin strategy set!");
                return;
            }

            _strategy.ExecuteSpin(
                wheelView.Animation,
                _currentSlices,
                _sliceCount,
                spinRotations,
                minDuration,
                maxDuration,
                OnSpinCompleted
            );
        }

        private void OnSpinCompleted(RewardItem result)
        {
            IsSpinning = false;
            wheelView.HighlightIndicator();
            Publish(new WheelSpinStoppedEvent(result));
        }

        private void SetStrategy(WheelType type)
        {
            _strategy = WheelStrategyFactory.Create(type);
            if (_strategy == null)
                Debug.LogError($"[WheelManager] No strategy found for {type} type.");
        }

        private void SetWheelConfig(WheelType type)
        {
            if (!_wheelConfigs.TryGetValue(type, out var config))
                return;

            if (_currentConfig == config)
                return;

            _currentConfig = config;
            wheelView.UpdateWheelView(config);
        }

        public void ResetWheel()
        {
            wheelView.ResetWheel();
            IsSpinning = false;
        }
        #endregion
    }
}

