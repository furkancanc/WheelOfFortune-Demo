using TMPro;
using UnityEngine;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class ZoneCounterView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField, Tooltip("Text element that displays the zone number.")]
        private TextMeshProUGUI numberText;

        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = transform as RectTransform;
        }

        public void Setup(int zoneIndex, Color color)
        {
            if (numberText == null)
            {
                Debug.LogError($"{nameof(ZoneCounterView)}: Missing reference to NumberText.", this);
                return;
            }

            numberText.text = zoneIndex.ToString();
            numberText.color = color;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            TryAutoAssign(ref numberText, "");
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
    }
}
