using System.Collections.Generic;
using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // SpawnManager — spawna fileiras de obstáculos, correntes de ar e
    // cenário decorativo além da névoa (z = 70), gerencia o despawn e o
    // bônus de score por obstáculo ultrapassado. Usa ObjectPool por chave.
    // ---------------------------------------------------------------------
    public sealed class SpawnManager
    {
        private sealed class ActiveSpawn
        {
            public GameObject Go;
            public ObjectPool Pool;
            public bool IsObstacle;
            public bool Passed;
        }

        private static readonly int[] Lanes = { -6, -3, 0, 3, 6 };
        private const float SpawnZ = 70f;
        private const float DespawnZ = -14f;

        private readonly List<ActiveSpawn> _active = new List<ActiveSpawn>();
        private readonly Dictionary<string, ObjectPool> _pools =
            new Dictionary<string, ObjectPool>();

        private Transform _root;
        private DifficultyManager _difficulty;
        private ScoreManager _score;
        private float _obstacleTimer;
        private float _windTimer;
        private float _decorTimer;
        private bool _running;

        public void Init(Transform worldRoot, DifficultyManager difficulty, ScoreManager score)
        {
            _root = worldRoot;
            _difficulty = difficulty;
            _score = score;

            Prewarm("building", ObstacleFactory.CreateBuilding, 6);
            Prewarm("antenna", ObstacleFactory.CreateAntenna, 6);
            Prewarm("wire", ObstacleFactory.CreateWire, 6);
            Prewarm("billboard", ObstacleFactory.CreateBillboard, 6);
            Prewarm("tree", ObstacleFactory.CreateTree, 6);
            Prewarm("wind_blue", () => ObstacleFactory.CreateWind(WindType.Ascending), 3);
            Prewarm("wind_red", () => ObstacleFactory.CreateWind(WindType.Turbulence), 3);
            Prewarm("decor", ObstacleFactory.CreateDecorBuilding, 12);
            Prewarm("stripe", ObstacleFactory.CreateRoadStripe, 16);
        }

        private void Prewarm(string key, System.Func<GameObject> factory, int count)
        {
            _pools[key] = new ObjectPool(key, factory, count);
        }

        public void BeginRun()
        {
            ClearActive();
            _running = true;
            _obstacleTimer = 1.3f;
            _windTimer = 4.5f;
            _decorTimer = 0.05f;
        }

        public void Stop()
        {
            _running = false;
        }

        public void Tick(float dt)
        {
            if (!_running) return;

            _obstacleTimer -= dt;
            if (_obstacleTimer <= 0f)
            {
                SpawnObstacleRow();
                _obstacleTimer = _difficulty.SpawnInterval;
            }

            _windTimer -= dt;
            if (_windTimer <= 0f)
            {
                SpawnWind();
                _windTimer = _difficulty.WindInterval * Random.Range(0.8f, 1.25f);
            }

            _decorTimer -= dt;
            if (_decorTimer <= 0f)
            {
                SpawnDecorRow();
                _decorTimer = 0.5f;
            }

            DespawnAndScore();
        }

        // ------------------------------------------------------------ spawn

        private void SpawnObstacleRow()
        {
            float z = SpawnZ + Random.Range(0f, 6f);

            if (Random.value < _difficulty.DoubleObstacleChance)
            {
                // dois obstáculos com corredor garantido (distância >= 2 lanes)
                int i = Random.Range(0, 3);
                int j = i + Random.Range(2, 5 - i);
                SpawnObstacle(RandomObstacleKey(false), Lanes[i], z);
                SpawnObstacle(RandomObstacleKey(true), Lanes[j], z);
            }
            else
            {
                SpawnObstacle(RandomObstacleKey(false), Lanes[Random.Range(0, Lanes.Length)], z);
            }
        }

        private string RandomObstacleKey(bool avoidWire)
        {
            string[] keys = ObstacleFactory.ObstaclePoolKeys;
            string key = keys[Random.Range(0, keys.Length)];
            if (avoidWire && key == "wire")
            {
                // re-sorteia entre os 4 tipos restantes (sem fio duplicado)
                int r = Random.Range(0, 4);
                key = r == 0 ? "building"
                    : r == 1 ? "antenna"
                    : r == 2 ? "billboard"
                    : "tree";
            }
            return key;
        }

        private void SpawnObstacle(string key, int laneX, float z)
        {
            // o fio atravessa a pista inteira: sempre no centro
            float x = key == "wire" ? 0f : laneX;
            SpawnFrom(key, x, z);
        }

        private void SpawnWind()
        {
            string key = Random.value < 0.55f ? "wind_blue" : "wind_red";
            SpawnFrom(key, Random.Range(-5f, 5f), SpawnZ + 5f);
        }

        private void SpawnDecorRow()
        {
            float z = SpawnZ + Random.Range(0f, 10f);

            SpawnFrom("stripe", Random.Range(-0.4f, 0.4f), z);
            if (Random.value < 0.5f)
                SpawnFrom("stripe", Random.value < 0.5f ? -9f : 9f, z + Random.Range(-3f, 3f));

            if (Random.value < 0.85f)
                SpawnFrom("decor", Random.Range(15f, 27f) * (Random.value < 0.5f ? -1f : 1f),
                    z + Random.Range(-4f, 4f));
            if (Random.value < 0.85f)
                SpawnFrom("decor", Random.Range(15f, 27f) * (Random.value < 0.5f ? -1f : 1f),
                    z + Random.Range(-4f, 4f));
        }

        private void SpawnFrom(string key, float x, float z)
        {
            ObjectPool pool = _pools[key];
            GameObject go = pool.Get(new Vector3(x, 0f, z), _root);
            _active.Add(new ActiveSpawn
            {
                Go = go,
                Pool = pool,
                IsObstacle = key == "building" || key == "antenna" ||
                             key == "wire" || key == "billboard" || key == "tree"
            });
        }

        // ----------------------------------------------------------- ciclo

        private void DespawnAndScore()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ActiveSpawn a = _active[i];
                float wz = a.Go.transform.position.z;

                if (wz < DespawnZ)
                {
                    a.Pool.Release(a.Go);
                    _active.RemoveAt(i);
                    continue;
                }

                if (a.IsObstacle && !a.Passed && wz < 0f)
                {
                    a.Passed = true;
                    _score.AddObstacleBonus();
                }
            }
        }

        private void ClearActive()
        {
            for (int i = 0; i < _active.Count; i++)
                _active[i].Pool.Release(_active[i].Go);
            _active.Clear();
        }
    }
}
