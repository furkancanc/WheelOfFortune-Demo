using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Game.Data
{
    [Serializable]
    public class RoundSliceSet
    {
        [Range(1, 60)]
        public int Round;

        [Tooltip("The reward slices for this round.")]
        public List<RewardItem> Slices = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Round < 1)
                Round = 1;
        }
#endif
    }

    [CreateAssetMenu(menuName = "WheelOfFortune/WheelRoundData")]
    public class WheelRoundData : ScriptableObject
    {
        [Tooltip("Define slices per round. Each round can have different rewards.")]
        [SerializeField] private List<RoundSliceSet> roundSliceSets = new();

        private readonly Dictionary<int, List<RewardItem>> _roundCache = new();

        public RewardItem[] GetSlicesForRound(int round)
        {
            if (round < 1)
                round = 1;

            if (_roundCache.TryGetValue(round, out var slices))
                return slices.ToArray();

            for (int i = round - 1; i >= 1; i--)
            {
                if (_roundCache.TryGetValue(i, out slices))
                    return slices.ToArray();
            }

            if (_roundCache.Count > 0)
                return _roundCache[GetLowestRound()].ToArray();

            Debug.LogError("[WheelRoundData] No slice data found for any round!");
            return Array.Empty<RewardItem>();
        }

        public void InitializeCache()
        {
            _roundCache.Clear();

            foreach (var set in roundSliceSets)
            {
                if (set == null || set.Slices == null || set.Slices.Count == 0)
                {
                    Debug.LogWarning($"[WheelRoundData] Empty RoundSliceSet detected (Round: {set?.Round}). Skipping.");
                    continue;
                }

                if (!_roundCache.TryGetValue(set.Round, out var list))
                {
                    list = new List<RewardItem>();
                    _roundCache[set.Round] = list;
                }

                list.AddRange(set.Slices);
            }
        }

        private int GetLowestRound()
        {
            int min = int.MaxValue;
            foreach (var key in _roundCache.Keys)
                if (key < min)
                    min = key;
            return min;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (roundSliceSets == null) return;
            roundSliceSets.RemoveAll(set => set == null);

            int round = 1;
            foreach (var set in roundSliceSets)
            {
                set.Round = round;
                round++;
            }
        }
#endif
    }
}
