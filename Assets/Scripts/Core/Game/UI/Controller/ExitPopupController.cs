using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Infrastructure.Events;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class ExitPopupController : MonoBehaviour
    {
        [Header("Button References (Auto-assigned)")]
        [SerializeField] private Button collectButton;
        [SerializeField] private Button goBackButton;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!collectButton)
                collectButton = transform.Find("CollectButton")?.GetComponent<Button>()
                                   ?? GetComponentInChildren<Button>(true);

            if (!goBackButton)
                goBackButton = transform.Find("GoBackButton")?.GetComponent<Button>()
                                   ?? GetComponentInChildren<Button>(true);
        }
#endif

        private void Awake()
        {
            collectButton.onClick.AddListener(OnCollectClicked);
            goBackButton.onClick.AddListener(OnGoBackClicked);
        }

        private void OnDestroy()
        {
            collectButton.onClick.RemoveListener(OnCollectClicked);
            goBackButton.onClick.RemoveListener(OnGoBackClicked);
        }

     
        private void OnCollectClicked()
        {
            EventBus.Publish(new CollectRewardsButtonClickedEvent());
        }

        private void OnGoBackClicked()
        {
            EventBus.Publish(new ExitButtonClickedEvent());
        }
    }
}