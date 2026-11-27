namespace PigeonRooftopRush
{


    public sealed class DifficultyManager
    {
        private const float SpeedMin = 9f;
        private const float SpeedMax = 24f;
        private const float SpeedRampSeconds = 75f;
        private const float OtherDifficultyRampSeconds = 150f;
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
            float tDifficulty = Clamp01(Elapsed / OtherDifficultyRampSeconds);

            Speed = Lerp(SpeedMin, SpeedMax, tSpeed);
            SpawnInterval = Lerp(SpawnMin, SpawnMax, tSpawn);
            DoubleObstacleChance = Lerp(DoubleMin, DoubleMax, tDifficulty);
            WindInterval = Lerp(WindMin, WindMax, tDifficulty);
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
