using DG.Tweening;
using UnityEngine;
using WheelOfFortune.Infrastructure.Events;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.UI
{
    public class ExitPopup : MonoBehaviour
    {
        [Header("UI References (Auto-assigned)")]
        [SerializeField, Tooltip("CanvasGroup controlling popup visibility.")]
        private CanvasGroup canvasGroup;

        [SerializeField, Tooltip("Handles popup animations.")]
        private ExitPopupAnimation popupAnimation;

        [SerializeField, Tooltip("Handles logic for exit popup buttons.")]
        private ExitPopupController controller;

        private bool _isOpen;

        #region UNITY LIFECYCLE

#if UNITY_EDITOR
        private void OnValidate()
        {
            TryAutoAssign(ref canvasGroup, "Exit-Popup");
            TryAutoAssign(ref popupAnimation, "Animation");
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
        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponentInChildren<CanvasGroup>(true) ?? gameObject.AddComponent<CanvasGroup>();

            if (popupAnimation == null)
                popupAnimation = GetComponentInChildren<ExitPopupAnimation>(true);

            if (controller == null)
                controller = GetComponentInChildren<ExitPopupController>(true);

            popupAnimation?.InjectDependency(canvasGroup, transform);

            gameObject.SetActive(false);
            canvasGroup.alpha = 0f;
            _isOpen = false;
        }

        private void OnEnable()
        {
            Subscribe<ExitButtonClickedEvent>(OnExitButtonClicked);
            Subscribe<CollectRewardsButtonClickedEvent>(OnCollectRewardsButtonClicked);
        }

        private void OnDisable()
        {
            Unsubscribe<ExitButtonClickedEvent>(OnExitButtonClicked);
            Unsubscribe<CollectRewardsButtonClickedEvent>(OnCollectRewardsButtonClicked);

            popupAnimation?.KillActiveTweens();
        }

        private void OnDestroy()
        {
            popupAnimation?.KillActiveTweens();
            Unsubscribe<ExitButtonClickedEvent>(OnExitButtonClicked);
            Unsubscribe<CollectRewardsButtonClickedEvent>(OnCollectRewardsButtonClicked);
        }
        #endregion

        #region EVENT HANDLERS

        private void OnExitButtonClicked(ExitButtonClickedEvent _) => Hide();
        private void OnCollectRewardsButtonClicked(CollectRewardsButtonClickedEvent _) => Hide();

        #endregion

        #region PUBLIC API
        public void Show()
        {
            if(_isOpen)
                    return;

            _isOpen = true;
            gameObject.SetActive(true);
            popupAnimation?.PlayShow(); ;
        }

        private void Hide()
        {
            if (!_isOpen)
                return;

            _isOpen = false;
            popupAnimation?.PlayHide(() =>
            {
                if (this != null && gameObject != null)
                    gameObject.SetActive(false);
            });
        }
        #endregion

    }
}
