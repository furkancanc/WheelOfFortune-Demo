using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Infrastructure.Events;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class WheelUIController : MonoBehaviour
    {
        [Header("References (Auto-assigned)")]
        [SerializeField] private Button spinButton;

        #region Unity Lifecycle
        private void Awake()
        {
            ValidateReferences();
            SubscribeEvents();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!spinButton)
                spinButton = GetComponentInChildren<Button>(true);
        }
#endif
        #endregion

        #region Event Setup
        private void SubscribeEvents()
        {
            if (spinButton != null)
                spinButton.onClick.AddListener(OnSpinClicked);

            EventBus.Subscribe<RoundAdvancedEvent>(OnRoundAdvanced);
            EventBus.Subscribe<ReviveButtonClicked>(OnReviveButonClicked);
        }

     
        private void UnsubscribeEvents()
        {
            if (spinButton != null)
                spinButton.onClick.RemoveListener(OnSpinClicked);

            EventBus.Unsubscribe<RoundAdvancedEvent>(OnRoundAdvanced);
            EventBus.Unsubscribe<ReviveButtonClicked>(OnReviveButonClicked);
        }
        #endregion

        #region UI Interaction
        private void OnSpinClicked()
        {
            SetSpinButtonState(false);
            EventBus.Publish(new WheelSpinStartRequested());
        }

        private void OnRoundAdvanced(RoundAdvancedEvent _)
        {
            SetSpinButtonState(true);
        }

        private void OnReviveButonClicked(ReviveButtonClicked clicked)
        {
            SetSpinButtonState(true);
        }

        private void SetSpinButtonState(bool interactable)
        {
            if (spinButton == null) return;
            spinButton.interactable = interactable;
        }
        #endregion

        #region Validation
        private void ValidateReferences()
        {
            if (spinButton == null)
                LogError("SpinButton reference missing!");
        }
        #endregion

        #region Logging
        private static void LogError(string msg) => Debug.LogError($"[WheelUIController] {msg}");
        private static void LogWarning(string msg) => Debug.LogWarning($"[WheelUIController] {msg}");
        #endregion
    }
}