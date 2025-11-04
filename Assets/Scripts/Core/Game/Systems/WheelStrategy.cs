using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Game.Data;
using WheelOfFortune.Game.UI;

namespace WheelOfFortune.Game.Systems
{
    public interface IWheelStrategy
    {
        void ExecuteSpin(
            WheelAnimation animation,
            RewardItem[] slices,
            int sliceCount,
            int spinRotations,
            float minDuration,
            float maxDuration,
            System.Action<RewardItem> onComplete);
    }

    public class BronzeWheelStrategy : IWheelStrategy
    {
        public void ExecuteSpin(
            WheelAnimation animation,
            RewardItem[] slices,
            int sliceCount,
            int spinRotations,
            float minDuration,
            float maxDuration,
            System.Action<RewardItem> onComplete)
        {
            if (sliceCount == 0)
            {
                Debug.LogError("[BronzeWheelStrategy] Slice count is zero!");
                return;
            }

            int sliceIndex = Random.Range(0, sliceCount);
            int correctedIndex = (sliceCount - 1 - sliceIndex);

            float perSlice = 360f / sliceCount;
            float baseRotation = 360f * spinRotations;
            float targetAngle = baseRotation + (360f - correctedIndex * perSlice);

            
            float duration = Random.Range(minDuration, maxDuration);

            animation.AnimateSpin(duration, targetAngle, () =>
            {
                RewardItem result = slices[correctedIndex];
                onComplete?.Invoke(result);
            });
        }
    }

    public class SilverWheelStrategy : IWheelStrategy
    {
        public void ExecuteSpin(
            WheelAnimation animation,
            RewardItem[] slices,
            int sliceCount,
            int spinRotations,
            float minDuration,
            float maxDuration,
            System.Action<RewardItem> onComplete)
        {
            if (sliceCount == 0)
            {
                Debug.LogError("[SilverWheelStrategy] Slice count is zero!");
                return;
            }

            int sliceIndex = Random.Range(0, sliceCount);
            int correctedIndex = (sliceCount - 1 - sliceIndex);

            float perSlice = 360f / sliceCount;
            float targetAngle = (360f * (spinRotations + 1)) + (360f - correctedIndex * perSlice);
            float duration = Random.Range(minDuration, maxDuration) * 1.2f;

            animation.AnimateSpin(duration, targetAngle, () =>
            {
                RewardItem result = slices[correctedIndex];
                onComplete?.Invoke(result);
            });
        }
    }

    public class GoldWheelStrategy : IWheelStrategy
    {
        public void ExecuteSpin(
            WheelAnimation animation,
            RewardItem[] slices,
            int sliceCount,
            int spinRotations,
            float minDuration,
            float maxDuration,
            System.Action<RewardItem> onComplete)
        {
            if (sliceCount == 0)
            {
                Debug.LogError("[GoldWheelStrategy] Slice count is zero!");
                return;
            }

            int sliceIndex = Random.Range(0, sliceCount);
            int correctedIndex = (sliceCount - 1 - sliceIndex);

            float perSlice = 360f / sliceCount;
            float targetAngle = (360f * (spinRotations + 2)) + (360f - correctedIndex * perSlice);
            float duration = Random.Range(minDuration, maxDuration) * 1.3f;

            animation.AnimateSpin(duration, targetAngle, () =>
            {
                RewardItem result = slices[correctedIndex];
                onComplete?.Invoke(result);
            });
        }
    }
}
