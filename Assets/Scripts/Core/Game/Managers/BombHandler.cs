using UnityEngine;
using WheelOfFortune.Game.UI;
using WheelOfFortune.Infrastructure.Events;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.Managers
{
    [DisallowMultipleComponent]
    public class BombHandler : MonoBehaviour
    {
        [Header("UI References (Auto-assigned)")]
        [SerializeField] private BombPopupView bombPopupPrefab;
        [SerializeField] private Transform uiParent;

        private BombPopupView _popupInstance;

        #region UNITY LIFECYCLE

        private void Awake()
        {
            TryResolveDependencies();
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (uiParent == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                    uiParent = canvas.transform;
                else
                    Debug.LogWarning($"{nameof(BombHandler)}: No Canvas found in parent hierarchy.", this);

                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (bombPopupPrefab == null)
            {
                bombPopupPrefab = Resources.Load<BombPopupView>("Prefabs/ui_bomb-popup");
                if (bombPopupPrefab == null)
                    Debug.LogWarning($"{nameof(BombHandler)}: Could not auto-load BombPopup prefab from Resources/UI.", this);
            }
        }
#endif
        #endregion

        #region DEPENDENCY RESOLUTION

        private void TryResolveDependencies()
        {
            if (uiParent == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                uiParent = canvas ? canvas.transform : transform;
            }

            if (bombPopupPrefab == null)
            {
                bombPopupPrefab = Resources.Load<BombPopupView>("Prefabs/ui_bomb-popup");
                if (bombPopupPrefab == null)
                    Debug.LogError($"{nameof(BombHandler)}: BombPopup prefab not found in Resources/Prefabs/ui_bomb-popup.", this);
            }
        }

        #endregion

        #region Event Handling
        private void SubscribeEvents()
        {
            EventBus.Subscribe<BombTriggeredEvent>(OnBombTriggered);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<BombTriggeredEvent>(OnBombTriggered);
        }

        private void OnBombTriggered(BombTriggeredEvent _)
        {
            ShowPopup();
        }
        #endregion

        #region Popup Handling
        private void ShowPopup()
        {
            if (bombPopupPrefab == null)
            {
                Debug.LogError($"{nameof(BombHandler)}: Cannot show popup — prefab is missing.", this);
                return;
            }

            if (_popupInstance == null)
            {
                _popupInstance = Instantiate(bombPopupPrefab, uiParent);
                _popupInstance.name = $"{nameof(BombPopupView)}_Instance";
            }

            if (!_popupInstance.gameObject.activeSelf)
                _popupInstance.gameObject.SetActive(true);

            _popupInstance.Show();
        }
        #endregion
    }
}
