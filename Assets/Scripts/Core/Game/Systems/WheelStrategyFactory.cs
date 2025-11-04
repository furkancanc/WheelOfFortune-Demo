using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Game.Data;

namespace WheelOfFortune.Game.Systems
{
    public static class WheelStrategyFactory
    {
        public static IWheelStrategy Create(WheelType type)
        {
            return type switch
            {
                WheelType.Bronze    => new BronzeWheelStrategy(),
                WheelType.Silver    => new SilverWheelStrategy(),
                WheelType.Gold      => new GoldWheelStrategy(),
                _                   => new BronzeWheelStrategy()
            };
        }
    }
}
