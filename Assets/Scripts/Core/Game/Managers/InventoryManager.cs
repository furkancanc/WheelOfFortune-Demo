using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Game.Data;
using WheelOfFortune.Game.UI;
using WheelOfFortune.Infrastructure.Events;
using WheelOfFortune.Infrastructure.Pooling;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.Managers
{
    public interface IInventoryService
    {
        InventorySlot AddOrUpdateSlot(RewardItem reward, out bool wasNew);
    }

    [DisallowMultipleComponent]
    public class InventoryManager : MonoBehaviour, IInventoryService
    {
        [Header("References (Auto-assigned)")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private InventorySlot slotPrefab;

        [Header("Pooling Settings")]
        [SerializeField, Min(1)] private int initialPoolSize = 10;

        private readonly Dictionary<string, InventorySlot> _slots = new();
        private PoolManager<InventorySlot> _pool;

        #region UNITY LIFECYCLE
        private void Awake()
        {
            ValidateReferences();
            InitializePool();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<RewardCollectedEvent>(OnRewardCollected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<RewardCollectedEvent>(OnRewardCollected);
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
        private void OnRewardCollected(RewardCollectedEvent e)
        {
            if (e == null || e.RewardData == null)
                return;

            AddOrUpdateSlot(e.RewardData, out bool wasNew);
        }
        #endregion

        #region INVENTORY MANAGEMENT
        public InventorySlot AddOrUpdateSlot(RewardItem data, out bool wasNew)
        {
            if (data == null || string.IsNullOrEmpty(data.Id))
            {
                Debug.LogError("Invalid reward data passed to AddOrUpdateSlot.");
                wasNew = false;
                return null;
            }

            if (_slots.TryGetValue(data.Id, out var existingSlot))
            {
                wasNew = false;
                return existingSlot;
            }

            var slot = GetSlotFromPool();
            slot.Setup(data);
            slot.transform.SetParent(contentParent, false);
            slot.gameObject.SetActive(true);

            _slots[data.Id] = slot;
            wasNew = true;

            return slot;
        }

        private void InitializePool()
        {
            if (slotPrefab == null || contentParent == null)
            {
                Debug.LogError("Cannot initialize pool — missing references.");
                return;
            }

            _pool = new PoolManager<InventorySlot>(slotPrefab, initialPoolSize, contentParent);
        }

        private InventorySlot GetSlotFromPool() => _pool?.Get();

        #endregion

        #region VALIDATION & LOGGING
        private void ValidateReferences()
        {
            if (!contentParent)
                Debug.LogError("ContentParent is missing — will attempt to auto-link in Editor.");

            if (!slotPrefab)
                Debug.LogError("SlotPrefab is missing — assign via inspector or ensure prefab exists.");
        }

#if UNITY_EDITOR
        private void AutoAssignReferences()
        {
            if (!contentParent)
            {
                var scroll = GetComponentInChildren<ScrollRect>(true);
                if (scroll && scroll.content)
                    contentParent = scroll.content;
                else
                {
                    var inventory = transform.Find("ContentParent");
                    if (inventory)
                        contentParent = inventory;
                }
            }

            if (!slotPrefab)
                slotPrefab = Resources.Load<InventorySlot>("Prefabs/ui_inventroy-slot");
        }
#endif
        #endregion
    }
}
