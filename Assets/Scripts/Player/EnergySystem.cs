using UnityEngine;

namespace PigeonRooftopRush
{


    public sealed class EnergySystem
    {
        public const float Max = 100f;

        private const float CruiseDrain = 1.5f;
        private const float GlideDrain = 0.7f;
        private const float FlapCost = 3f;
        private const float TurbulenceCost = 25f;

        public float Current { get; private set; } = Max;
        public float Normalized => Current / Max;
        public bool FlapBlocked => Current <= 0.01f;

        public WindType Wind { get; private set; } = WindType.None;
        public bool InWind { get; private set; }

        private int _lastPercent = 100;

        public void Reset()
        {
            Current = Max;
            Wind = WindType.None;
            InWind = false;
            _lastPercent = -1;
            RaiseEnergy();
        }

        public void SetWind(WindType type, bool inside)
        {
            Wind = inside ? type : WindType.None;
            InWind = inside && type != WindType.None;
            if (InWind && Wind == WindType.Ascending)
            {
                Current = Max;
                RaiseEnergy();
            }
            else if (InWind && Wind == WindType.Turbulence)
            {
                Current = Mathf.Max(0f, Current - TurbulenceCost);
                RaiseEnergy();
            }
        }


        public bool TryConsumeFlap()
        {
            if (FlapBlocked) return false;
            Current = Mathf.Max(0f, Current - FlapCost);
            RaiseEnergy();
            return true;
        }

        public void Tick(float dt, bool gliding)
        {
            if (InWind && Wind == WindType.Ascending)
            {
                Current = Max;
            }
            else if (!InWind || Wind != WindType.Turbulence)
            {
                Current -= (gliding ? GlideDrain : CruiseDrain) * dt;
            }

            Current = Mathf.Clamp(Current, 0f, Max);
            RaiseEnergy();
        }

        private void RaiseEnergy()
        {
            int percent = Mathf.Clamp(Mathf.RoundToInt(Current), 0, 100);
            if (percent == _lastPercent) return;
            _lastPercent = percent;
            EventBus.Raise(new EnergyChangedEvent { Percent = percent });
        }
    }
}
