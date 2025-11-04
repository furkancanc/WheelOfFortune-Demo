using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Game.Managers;
using WheelOfFortune.Infrastructure.Events;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class ExitButtonController : MonoBehaviour
    {
        [SerializeField] private Button exitButton;
        [SerializeField] private ExitPopup exitPopupPrefab;
        [SerializeField] private Transform uiRoot;

        [SerializeField] private MonoBehaviour gameStateProviderBehaviour;
        private IGameStateProvider _gameStateProvider;
        private ExitPopup _exitPopupInstance;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!exitButton)
                exitButton = GetComponent<Button>();
            if (!uiRoot)
                uiRoot = GetComponentInParent<Canvas>()?.transform;
            if (!exitPopupPrefab)
                exitPopupPrefab = Resources.Load<ExitPopup>("Prefabs/ui_exit-popup");
        }
#endif

        private void Awake()
        {
            _gameStateProvider = gameStateProviderBehaviour as IGameStateProvider;

            if (exitButton)
                exitButton.onClick.AddListener(OnExitClicked);

            exitButton.interactable = false;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<RoundAdvancedEvent>(OnRoundChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<RoundAdvancedEvent>(OnRoundChanged);
        }

        private void OnRoundChanged(RoundAdvancedEvent evt)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (_gameStateProvider == null)
            {
                exitButton.interactable = false;
                return;
            }

            bool canExit = (_gameStateProvider.IsSafeZone || _gameStateProvider.IsSuperZone)
                           && !_gameStateProvider.IsWheelSpinning;

            exitButton.interactable = canExit;
        }

        private void OnExitClicked()
        {
            if (_gameStateProvider == null || _gameStateProvider.IsWheelSpinning)
                return;

            if (_exitPopupInstance == null)
                _exitPopupInstance = Instantiate(exitPopupPrefab, uiRoot);

            _exitPopupInstance.Show();
        }
    }
}
