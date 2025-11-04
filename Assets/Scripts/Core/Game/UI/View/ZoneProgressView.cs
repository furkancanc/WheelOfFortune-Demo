
using TMPro;
using UnityEngine;
using WheelOfFortune.Infrastructure.Events;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class ZoneProgressView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField, Tooltip("Text field showing current Safe Zone count.")]
        private TextMeshProUGUI safeZoneText;

        [SerializeField, Tooltip("Text field showing current Super Zone count.")]
        private TextMeshProUGUI superZoneText;

        private static readonly System.Text.StringBuilder sb = new(8);

        #region Unity Lifecycle

        private void OnEnable()
        {
            EventBus.Subscribe<SafeZoneUpdatedEvent>(OnSafeZoneUpdated);
            EventBus.Subscribe<SuperZoneUpdatedEvent>(OnSuperZoneUpdated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SafeZoneUpdatedEvent>(OnSafeZoneUpdated);
            EventBus.Unsubscribe<SuperZoneUpdatedEvent>(OnSuperZoneUpdated);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            TryAutoAssign(ref safeZoneText, "ui_zone-progress_safe-zone/ui_zone-progress_safe-zone_count_value");
            TryAutoAssign(ref superZoneText, "ui_zone-progress_super-zone/ui_zone-progress_super-zone_count_value");
        }

        private void TryAutoAssign(ref TextMeshProUGUI target, string childPath)
        {
            if (target != null)
                return;

            Transform child = transform.Find(childPath);
            if (child == null)
                return;

            TextMeshProUGUI found;
            if (child.TryGetComponent(out found))
                target = found;
        }
#endif

        #endregion

        #region Event Handlers

        private void OnSafeZoneUpdated(SafeZoneUpdatedEvent e)
        {
            UpdateZoneText(safeZoneText, e.NextSafeZone);
        }

        private void OnSuperZoneUpdated(SuperZoneUpdatedEvent e)
        {
            UpdateZoneText(superZoneText, e.NextSuperZone);
        }

        #endregion

        #region Public API
        public void UpdateZoneTexts(int safeZoneCount, int superZoneCount)
        {
            UpdateZoneText(safeZoneText, safeZoneCount);
            UpdateZoneText(superZoneText, superZoneCount);
        }

        #endregion

        #region Private Helpers
        private static void UpdateZoneText(TextMeshProUGUI text, int value)
        {
            if (text == null)
                return;

            sb.Clear();
            sb.Append(value);
            text.SetText(sb);
        }

        #endregion
    }
}
