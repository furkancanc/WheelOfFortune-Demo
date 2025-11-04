using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Game.Data
{
    [CreateAssetMenu(menuName = "WheelOfFortune/ZoneConfig")]
    public class ZoneConfig : ScriptableObject
    {
        public int MaximumZoneCount = 60;
        public int SafeZoneInterval = 5;
        public int SuperZoneInterval = 30;
    }

    [System.Serializable]
    public class ZoneVisuals
    {
        public Sprite Background;
        public Color TextColor;
    }

    public enum ZoneType
    {
        Normal,
        Safe,
        Super
    }
}
