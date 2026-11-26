using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // ScoreManager — score = distância (m) + 25 por obstáculo ultrapassado.
    // Recorde persistido em PlayerPrefs (PRR_BestScore).
    // ---------------------------------------------------------------------
    public sealed class ScoreManager
    {
        private const string BestKey = "PRR_BestScore";
        private const int ObstacleBonus = 25;

        private float _distanceFloat;

        public int Score { get; private set; }
        public int Distance { get; private set; }
        public int ObstaclesPassed { get; private set; }
        public int Best { get; private set; }

        public ScoreManager()
        {
            Best = PlayerPrefs.GetInt(BestKey, 0);
        }

        public void BeginRun()
        {
            _distanceFloat = 0f;
            Distance = 0;
            ObstaclesPassed = 0;
            Score = 0;
            Raise();
        }

        public void Tick(float dt, float speed)
        {
            _distanceFloat += speed * dt;
            int d = (int)_distanceFloat;
            int s = d + ObstaclesPassed * ObstacleBonus;
            if (d != Distance || s != Score)
            {
                Distance = d;
                Score = s;
                Raise();
            }
        }

        public void AddObstacleBonus()
        {
            ObstaclesPassed++;
            Score = Distance + ObstaclesPassed * ObstacleBonus;
            Raise();
        }

        /// <summary>Grava o recorde se for maior. Retorna true se novo recorde.</summary>
        public bool SubmitBest()
        {
            if (Score <= Best) return false;
            Best = Score;
            PlayerPrefs.SetInt(BestKey, Best);
            PlayerPrefs.Save();
            return true;
        }

        private void Raise()
        {
            EventBus.Raise(new ScoreChangedEvent
            {
                Score = Score,
                Distance = Distance,
                ObstaclesPassed = ObstaclesPassed
            });
        }
    }
}
