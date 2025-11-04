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
    public class InventorySlot : MonoBehaviour
    {
        [Header("UI References (Auto-assigned)")]
        [SerializeField, Tooltip("Icon representing the item in this slot.")]
        private Image iconImage;

        [SerializeField, Tooltip("Text element displaying item count.")]
        private TextMeshProUGUI countText;

        [SerializeField, Tooltip("Animation handler controlling slot visual feedback.")]
        private InventorySlotAnimation animationHandler;

        private int _count;
        private string _id;

        #region PROPERTIES

        public string Id => _id;
        public int Count => _count;
        public RectTransform IconRectTransform => iconImage ? iconImage.rectTransform : null;
        public Vector3 IconWorldPosition => iconImage ? iconImage.rectTransform.position : transform.position;

        #endregion

        #region UNITY LIFECYCLE


#if UNITY_EDITOR
        private void OnValidate()
        {
            TryAutoAssign(ref iconImage, "Icon");
            TryAutoAssign(ref countText, "Slot_Count_Value");
            TryAutoAssign(ref animationHandler, "Animation");
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
        private void OnDisable()
        {
            animationHandler?.KillAllTweens();
        }

        #endregion

        #region SETUP & STATE

        public void Setup(RewardItem data)
        {
            _id = data.Id;
            _count = data.Count;

            if (iconImage)
                iconImage.sprite = data.Icon;

            if (countText)
                countText.SetText(_count.ToString());

            if (animationHandler)
                animationHandler.InjectDependency(this);
            else
                Debug.LogError($"{nameof(InventorySlot)}: Missing InventorySlotAnimation reference.", this);
        }

        #endregion

        #region VISUAL FEEDBACK
        public void UpdateCount(int addAmount)
        {
            if (addAmount <= 0)
                return;

           
            animationHandler?.PlayCountIncrement(countText, addAmount);
            _count += addAmount;
        }

        public void Pulse()
        {
            animationHandler?.Pulse();
        }

        #endregion
    }
}
