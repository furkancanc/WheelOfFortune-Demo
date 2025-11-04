using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class ZoneIndicatorView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;

        public void UpdateZone(Sprite backgroundSprite)
        {
            if (backgroundImage == null)
            {
                Debug.LogError($"{nameof(ZoneIndicatorView)}: Missing reference to BackgroundImage.");
                return;
            }

            if (backgroundSprite == null)
            {
                Debug.LogError($"{nameof(ZoneIndicatorView)}: Attempted to assign a null background sprite.");
                return;
            }

            if (backgroundImage.sprite != backgroundSprite)
            {
                backgroundImage.sprite = backgroundSprite;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (backgroundImage == null)
            {
                TryAutoAssign(ref backgroundImage, "ui-zone-counter-current-zone_background/ui_zone-counter_current-zone_background_image_value");
            }
        }

        private void TryAutoAssign(ref Image target, string childPath)
        {
            if (target != null)
                return;

            Transform child = transform.Find(childPath);
            if (child == null)
                return;

            Image found;
            if (child.TryGetComponent(out found))
                target = found;
        }
#endif
    }
}
