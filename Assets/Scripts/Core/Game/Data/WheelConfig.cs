using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Game.Data
{
    [CreateAssetMenu(menuName="WheelOfFortune/WheelConfig")]
    public class WheelConfig : ScriptableObject
    {
        public Sprite WheelSprite;
        public Sprite IndicatorSprite;
        public Color TextColor;
        public string Title;
        public string Info;
    }

    public enum WheelType { Bronze, Silver, Gold};
}
