using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Game.Data
{
    [CreateAssetMenu(menuName = "WheelOfFortune/Items/GameItem")]
    public class GameItem : ScriptableObject
    {
        [Header("General Info")]
        public string Id;    
        public string DisplayName; 
        public Sprite Icon;

        [Header("Category")]
        public ItemCategory Category;

        [Header("Sub Types (optional per category)")]
        public ChestType ChestType;
        public CosmeticType CosmeticType;
        public CurrencyType CurrencyType;
        public UpgradeType UpgradeType;
        public WeaponSkinType WeaponSkinType;

        public string GetDisplayName()
        {
            return !string.IsNullOrEmpty(DisplayName)
                ? DisplayName
                : $"{Category} - {Id}";
        }

        public bool IsBomb => Category == ItemCategory.Bomb;
    }
}
