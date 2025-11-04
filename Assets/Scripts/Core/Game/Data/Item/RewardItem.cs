using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Game.Data
{
    [System.Serializable]
    public class RewardItem
    {
        [Header("Reward Reference")]
        public GameItem Item;

        [Header("Reward Properties")]
        public int Count = 1;
        public string Id => Item != null ? Item.Id : "none";
        public Sprite Icon => Item != null ? Item.Icon : null;
        public string DisplayName => Item != null ? Item.GetDisplayName() : "Unknown Item";
        public bool IsBomb => Item != null && Item.IsBomb;
    }
}