using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Game.Data;
using WheelOfFortune.Game.UI;
using WheelOfFortune.Infrastructure.Events;
using WheelOfFortune.Infrastructure.Pooling;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.Managers
{
    public interface IRewardManager
    {
        void ShowReward(RewardItem rewardItem);
    }

    [DisallowMultipleComponent]
    public class RewardManager : MonoBehaviour, IRewardManager
    {
        [Header("Prefabs & Parents (Auto-assigned)")]
        [SerializeField] private RewardPopup rewardPopupPrefab;
        [SerializeField] private Transform popupParent;
        [SerializeField] private FlyingIconView flyingIconPrefab;

        [Header("Dependencies")]
        [SerializeField] private MonoBehaviour inventoryServiceRef;

        [Header("Pooling Settings")]
        [SerializeField, Min(1)] private int popupPoolSize = 5;
        [SerializeField, Min(5)] private int flyingIconPoolSize = 20;

        private IInventoryService _inventoryService;
        private PoolManager<RewardPopup> _popupPool;
        private PoolManager<FlyingIconView> _flyingIconPool;
        private readonly List<FlyingIconView> _activeIcons = new();

        #region UNITY LIFECYCLE
        private void Awake()
        {
            ResolveDependencies();
            InitializePools();

        }

        private void OnEnable()
        {
            EventBus.Subscribe<WheelSpinStoppedEvent>(OnSpinStopped);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<WheelSpinStoppedEvent>(OnSpinStopped);
            ClearActiveIcons();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoAssignReferences();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion

        #region EVENT HANDLING

        private void OnSpinStopped(WheelSpinStoppedEvent e)
        {
            if (e?.Result == null)
                return;

            if (e.Result.IsBomb)
            {
                Publish(new BombTriggeredEvent());
                return;
            }

            ShowReward(e.Result);
        }
        #endregion

        #region REWARD FLOW
        public void ShowReward(RewardItem rewardItem)
        {
            if (rewardItem == null)
            {
                Debug.LogWarning("[RewardManager] Tried to show null reward item.");
                return;
            }

            var popup = _popupPool.Get();
            popup.Show(
                rewardItem,
                (data, iconPos) => PrepareFlyingIcons(data, iconPos),
                (data, iconPos) => ExecuteFlyToInventory(data),
                () => Publish(new RewardCollectedEvent(rewardItem))
            );
        }

        private void PrepareFlyingIcons(RewardItem data, Vector3 fromScreenPos)
        {
            if (data == null)
                return;

            var slot = _inventoryService.AddOrUpdateSlot(data, out bool wasNew);
            if (slot == null || wasNew)
                return;

            ClearActiveIcons();

            int iconCount = FlyingIconAnimation.Config?.IconCount ?? 10;

            for (int i = 0; i < iconCount; i++)
            {
                var icon = _flyingIconPool.Get();
                icon.Init(_flyingIconPool);
                icon.Setup(data.Icon, fromScreenPos);
                icon.PlayScatter(fromScreenPos);
                _activeIcons.Add(icon);
            }
        }

        private void ExecuteFlyToInventory(RewardItem data)
        {
            if (data == null || _activeIcons.Count == 0)
                return;

            var slot = _inventoryService.AddOrUpdateSlot(data, out bool wasNew);
            if (slot == null)
                return;

            Vector3 targetPos = slot.IconWorldPosition;
            int total = _activeIcons.Count;
            int completed = 0;

            foreach (var icon in _activeIcons)
            {
                icon.PlayFlyTo(targetPos, () =>
                {
                    completed++;
                    if (completed == total)
                    {
                        slot.UpdateCount(data.Count);
                        ClearActiveIcons();
                    }
                });
            }
        }

        private void ClearActiveIcons()
        {
            if (_activeIcons.Count == 0)
                return;

            foreach (var icon in _activeIcons)
            {
                if (icon != null)
                    icon.gameObject.SetActive(false);
            }

            _activeIcons.Clear();
        }

        #endregion

        #region INITIALIZATION
        private void InitializePools()
        {
            if (rewardPopupPrefab == null || popupParent == null)
            {
                Debug.LogError("[RewardManager] Missing RewardPopup references!");
                return;
            }

            _popupPool ??= new PoolManager<RewardPopup>(rewardPopupPrefab, popupPoolSize, popupParent);
            _flyingIconPool ??= new PoolManager<FlyingIconView>(flyingIconPrefab, flyingIconPoolSize, popupParent);
        }
        private void ResolveDependencies()
        {
            if (inventoryServiceRef is IInventoryService inventoryService)
            {
                _inventoryService = inventoryService;
            }
            else
            {
                _inventoryService = FindFirstObjectByType<InventoryManager>();
                if (_inventoryService == null)
                    Debug.LogError("[RewardManager] Missing valid IInventoryService reference!");
            }
        }

#if UNITY_EDITOR
        private void AutoAssignReferences()
        {
            if (popupParent == null)
            {
                popupParent = transform;
            }

            if (inventoryServiceRef == null)
            {
                inventoryServiceRef = FindFirstObjectByType<InventoryManager>();
            }

            if (rewardPopupPrefab == null)
                rewardPopupPrefab = Resources.Load<RewardPopup>("Prefabs/ui_reward-popup");

            if (flyingIconPrefab == null)
                flyingIconPrefab = Resources.Load<FlyingIconView>("Prefabs/ui_flying-icon");
        }
#endif

        #endregion
    }
}
