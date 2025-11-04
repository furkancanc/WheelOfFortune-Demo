using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WheelOfFortune.Infrastructure.Events;
using static WheelOfFortune.Infrastructure.Events.EventBus;
using System;


namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class BombPopupView : MonoBehaviour
    {
        [Header("Sub Components (Auto-assigned)")]
        [SerializeField, Tooltip("Handles popup animations.")]
        private BombPopupAnimation animationHandler;

        [SerializeField, Tooltip("Handles input and game logic.")]
        private BombPopupController controller;

        private bool _isOpen;

        #region UNITY LIFECYCLE

        private void Awake()
        {
            if (controller == null)
                Debug.LogWarning($"{nameof(BombPopupView)}: Missing controller reference.", this);

            if (animationHandler == null)
                Debug.LogWarning($"{nameof(BombPopupView)}: Missing animation handler reference.", this);

            controller?.Initialize(this);
            animationHandler?.InjectDependency(this);

            gameObject.SetActive(true);
            _isOpen = false;
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            animationHandler?.KillActiveTweens();
            UnsubscribeEvents();
        }

        #endregion

        #region EVENT BUS HANDLERS

        private void SubscribeEvents()
        {
            EventBus.Subscribe<ReviveButtonClicked>(OnReviveClicked);
            EventBus.Subscribe<GiveUpButtonClicked>(OnGiveUpClicked);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<ReviveButtonClicked>(OnReviveClicked);
            EventBus.Unsubscribe<GiveUpButtonClicked>(OnGiveUpClicked);
        }

        private void OnReviveClicked(ReviveButtonClicked _) => Hide();
        private void OnGiveUpClicked(GiveUpButtonClicked _) => Hide();

        #endregion

        #region PUBLIC API
        public void Show()
        {
            if (_isOpen)
                return;

            _isOpen = true;
            gameObject.SetActive(true);
            animationHandler?.PlayShowAnimation();
        }

        public void Hide()
        {
            if (!_isOpen)
                return;

            animationHandler?.PlayHideAnimation(() =>
            {
                _isOpen = false;
                if (this != null && gameObject != null)
                    gameObject.SetActive(false);
            });
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            TryAutoAssign(ref animationHandler, "Animation");
            TryAutoAssign(ref controller, "Controller");

            UnityEditor.EditorUtility.SetDirty(this);
        }

        private static void TryAutoAssign<T>(ref T target, string nameHint) where T : Component
        {
            if (target != null) return;

            foreach (var comp in Resources.FindObjectsOfTypeAll<T>())
            {
                if (comp.name.ToLower().Contains(nameHint.ToLower()))
                {
                    target = comp;
                    return;
                }
            }
        }
#endif
    }
}
