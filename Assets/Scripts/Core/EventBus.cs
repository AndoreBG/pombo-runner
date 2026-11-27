using System;
using System.Collections.Generic;

namespace PigeonRooftopRush
{


    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers =
            new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            if (Handlers.TryGetValue(typeof(T), out var existing))
                Handlers[typeof(T)] = Delegate.Combine(existing, handler);
            else
                Handlers[typeof(T)] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            if (!Handlers.TryGetValue(typeof(T), out var existing)) return;
            existing = Delegate.Remove(existing, handler);
            if (existing == null) Handlers.Remove(typeof(T));
            else Handlers[typeof(T)] = existing;
        }

        public static void Raise<T>(T evt)
        {
            if (!Handlers.TryGetValue(typeof(T), out var d)) return;

            ((Action<T>)d)?.Invoke(evt);
        }

        public static void ClearAll()
        {
            Handlers.Clear();
        }
    }


    public enum CrashReason
    {
        Obstacle,
        Ground
    }

    public struct RunStartedEvent
    {
        public int RunIndex;
    }

    public struct RunEndedEvent
    {
        public CrashReason Reason;
        public int Score;
        public bool IsNewBest;
    }

    public struct ScoreChangedEvent
    {
        public int Score;
        public int Distance;
        public int ObstaclesPassed;
    }

    public struct EnergyChangedEvent
    {
        public int Percent;
    }

    public struct FlapEvent
    {
        public float EnergyAfter;
    }

    public struct FlapBlockedEvent
    {
    }

    public struct WindChangedEvent
    {
        public WindType Type;
        public bool Entered;
    }

    public struct CrashEvent
    {
        public CrashReason Reason;
    }
}
