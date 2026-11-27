namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // DifficultyManager — progressão da run (curvas do plano §4.3):
    //   velocidade     9 -> 24 m/s em ~150 s
    //   spawn          1.55 s -> 0.62 s em ~120 s
    //   2 obstáculos   0% -> 45% em ~150 s
    //   correntes      8 s -> 5 s
    // ---------------------------------------------------------------------
    public sealed class DifficultyManager
    {
        private const float SpeedMin = 9f;
        private const float SpeedMax = 24f;
        private const float SpeedRampSeconds = 150f;
        private const float SpawnMin = 1.55f;
        private const float SpawnMax = 0.62f;
        private const float SpawnRampSeconds = 120f;
        private const float DoubleMin = 0f;
        private const float DoubleMax = 0.45f;
        private const float WindMin = 8f;
        private const float WindMax = 5f;

        public float Elapsed { get; private set; }
        public float Speed { get; private set; } = SpeedMin;
        public float SpawnInterval { get; private set; } = SpawnMin;
        public float WindInterval { get; private set; } = (float)WindMin;
        public float DoubleObstacleChance { get; private set; }

        public void BeginRun()
        {
            Elapsed = 0f;
            Recompute();
        }

        /// <summary>Avança o tempo e retorna a velocidade atual do mundo.</summary>
        public float Tick(float dt)
        {
            Elapsed += dt;
            Recompute();
            return Speed;
        }

        private void Recompute()
        {
            float tSpeed = Clamp01(Elapsed / SpeedRampSeconds);
            float tSpawn = Clamp01(Elapsed / SpawnRampSeconds);

            Speed = Lerp(SpeedMin, SpeedMax, tSpeed);
            SpawnInterval = Lerp(SpawnMin, SpawnMax, tSpawn);
            DoubleObstacleChance = Lerp(DoubleMin, DoubleMax, tSpeed);
            WindInterval = Lerp(WindMin, WindMax, tSpeed);
        }

        private static float Clamp01(float v)
        {
            return v < 0f ? 0f : (v > 1f ? 1f : v);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
