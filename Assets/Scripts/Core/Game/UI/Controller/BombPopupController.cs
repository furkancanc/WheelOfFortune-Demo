using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WheelOfFortune.Infrastructure.Events;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class BombPopupController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button closeButton;

        public void Initialize(BombPopupView view)
        {
            BindButtons();
        }

        private void BindButtons()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(HandleContinueClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void HandleContinueClicked()
        {
            EventBus.Publish(new ReviveButtonClicked());
        }

        private void HandleCloseClicked()
        {
            EventBus.Publish(new GiveUpButtonClicked());
        }

        private void OnDestroy()
        {
            if (continueButton != null)
                continueButton.onClick.RemoveListener(HandleContinueClicked);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (continueButton == null || closeButton == null)
            {
                var buttons = GetComponentsInChildren<Button>(true);
                if (buttons.Length >= 2)
                {
                    continueButton ??= buttons[0];
                    closeButton ??= buttons[1];
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
