using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Game.Data;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class WheelSlice : MonoBehaviour
    {
        [Header("UI References (Auto-assigned)")]
        [SerializeField, Tooltip("Item icon displayed in this slice.")]
        private Image iconImage;

        [SerializeField, Tooltip("Text displaying item count if greater than 1.")]
        private TextMeshProUGUI countText;

        #region Unity Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            TryAutoAssign(ref iconImage, "icon_value");
            TryAutoAssign(ref countText, "amount_value");
        }

        private static void TryAutoAssign<T>(ref T target, string nameHint) where T : Component
        {
            if (target != null) return;
            var found = FindChildComponentByName<T>(nameHint);
            if (found != null) target = found;
        }

        private static T FindChildComponentByName<T>(string nameHint) where T : Component
        {
            foreach (var comp in Resources.FindObjectsOfTypeAll<T>())
            {
                if (comp.name.ToLower().Contains(nameHint.ToLower()))
                    return comp;
            }
            return null;
        }
#endif

        #endregion

        #region Setup & State
        public void Setup(RewardItem data)
        {
            UpdateVisuals(data.Icon, data.Count);
        }

        private void UpdateVisuals(Sprite iconSprite, int itemCount)
        {
            if (!iconImage || !countText) return;

            iconImage.sprite = iconSprite;
            countText.text = itemCount >= 1 ? $"x{itemCount}" : string.Empty;
        }
        #endregion

    }
}
