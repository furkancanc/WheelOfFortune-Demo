using System.Collections;
using UnityEngine;

namespace WheelOfFortune.Game.Data
{
    [CreateAssetMenu(menuName = "WheelOfFortune/ZoneVisualsConfig")]
    public class ZoneVisualsConfig : ScriptableObject
    {
        [Header("Text Colors")]
        public Color normalZoneTextColor = Color.white;
        public Color safeZoneTextColor = Color.black;
        public Color superZoneTextColor = Color.black;
        public Color currentZoneTextColor = Color.black;
        [Range(0, 1)]
        public float passedZoneAlpha = 1f;

        [Header("Backgrounds")]
        public Sprite normalZoneBackground;
        public Sprite safeZoneBackground;
        public Sprite superZoneBackground;

        public Color GetTextColor(ZoneType zoneType, bool isCurrent, bool isPassed)
        {
            if (isCurrent && zoneType == ZoneType.Normal)
                return currentZoneTextColor;

            Color baseColor = zoneType switch
            {
                ZoneType.Safe => safeZoneTextColor,
                ZoneType.Super => superZoneTextColor,
                _ => normalZoneTextColor
            };

            if (isPassed)
            {
                baseColor.a *= passedZoneAlpha;
            }

            return baseColor;
        }

        public Sprite GetBackground(ZoneType zoneType)
        {
            return zoneType switch
            {
                ZoneType.Safe => safeZoneBackground,
                ZoneType.Super => superZoneBackground,
                _ => normalZoneBackground
            };
        }
    }
}