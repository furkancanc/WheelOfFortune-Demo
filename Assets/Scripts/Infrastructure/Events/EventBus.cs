using System;
using System.Collections.Generic;
using WheelOfFortune.Game.Data;

namespace WheelOfFortune.Infrastructure.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _eventTable = new();

        public static void Subscribe<T>(Action<T> callback)
        {
            if (_eventTable.TryGetValue(typeof(T), out var existing))
            {
                _eventTable[typeof(T)] = Delegate.Combine(existing, callback);
            }
            else
            {
                _eventTable[typeof(T)] = callback;
            }
        }

        public static void Unsubscribe<T>(Action<T> callback)
        {
            if (_eventTable.TryGetValue(typeof(T), out var existing))
            {
                _eventTable[typeof(T)] = Delegate.Remove(existing, callback);
            }
        }

        public static void Publish<T>(T eventData)
        {
            if (_eventTable.TryGetValue(typeof(T), out var existing))
            {
                (existing as Action<T>)?.Invoke(eventData);
            }
        }

        internal static void Subscribe<T>()
        {
            throw new NotImplementedException();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// EVENTS
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        // WHEEL EVENTS
        public class WheelSpinStartRequested { }
        public class WheelSpinStartedEvent { };
        public class WheelSpinStoppedEvent
        {
            public RewardItem Result;
            public WheelSpinStoppedEvent(RewardItem result) => Result = result;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        // ROUND EVENTS
        public class RoundAdvancedEvent
        {
            public int CurrentRound;
            public int NextSafe;
            public int NextSuper;
            public RoundAdvancedEvent(int current)
            {
                CurrentRound = current;
            }
        }

        // REWARD EVENTS

        public class RewardCollectedEvent
        {
            public RewardItem RewardData { get; }

            public RewardCollectedEvent(RewardItem data)
            {
                RewardData = data;
            }
        }


        public class CollectRewardsButtonClickedEvent { }
        public class ExitButtonClickedEvent { }

        // Bomb Pop Up
        public class ReviveButtonClicked { }
        public class GiveUpButtonClicked { }
        public class BombTriggeredEvent { }

        public class SafeZoneUpdatedEvent
        {
            public int NextSafeZone;
            public SafeZoneUpdatedEvent(int nextSafeZone)
            {
                NextSafeZone = nextSafeZone;
            }
        }

        public class SuperZoneUpdatedEvent
        {
            public int NextSuperZone;
            public SuperZoneUpdatedEvent(int nextSuperZone)
            {
                NextSuperZone = nextSuperZone;
            }
        }

    }
}