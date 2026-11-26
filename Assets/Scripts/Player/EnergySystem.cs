using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // EnergySystem — sistema de energia do MVP (tabela do GDD):
    //   voo normal  -3.0/s | tap -6 | planar -1.4/s
    //   azul        +9.0/s | vermelha -8.0/s
    //   zerada      sem flap (o jogador precisa alcançar uma corrente azul)
    // Displain p/ UI via EventBus (EnergyChangedEvent, percentual inteiro).
    // ---------------------------------------------------------------------
    public sealed class EnergySystem
    {
        public const float Max = 100f;

        private const float CruiseDrain = 3.0f;
        private const float GlideDrain = 1.4f;
        private const float FlapCost = 6f;
        private const float AscendingRecover = 9.0f;
        private const float TurbulenceDrain = 8.0f;

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
            _lastPercent = -1; // força o Raise: HUD volta a 100% na nova run
            RaiseEnergy();
        }

        public void SetWind(WindType type, bool inside)
        {
            Wind = inside ? type : WindType.None;
            InWind = inside && type != WindType.None;
        }

        /// <summary>Consome energia de um tap. Retorna false se sem energia.</summary>
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
                Current += AscendingRecover * dt;
            }
            else if (InWind && Wind == WindType.Turbulence)
            {
                Current -= TurbulenceDrain * dt;
            }
            else
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
