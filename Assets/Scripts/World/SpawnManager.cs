using System.Collections.Generic;
using UnityEngine;

namespace PigeonRooftopRush
{


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
        private bool _forceBlueWind;

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
            _forceBlueWind = true;
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


        private void SpawnObstacleRow()
        {
            float z = SpawnZ + Random.Range(0f, 6f);

            string key1 = RandomObstacleKey(false);

            if (key1 == "wire" || Random.value >= _difficulty.DoubleObstacleChance)
            {
                SpawnObstacle(key1, Lanes[Random.Range(0, Lanes.Length)], z);
                return;
            }

            string key2 = RandomObstacleKey(true);
            float requiredSeparation = ObstacleHalfWidth(key1) + ObstacleHalfWidth(key2) + 2f;


            var pairs = new List<Vector2Int>();
            for (int i = 0; i < Lanes.Length; i++)
                for (int j = 0; j < Lanes.Length; j++)
                    if (Mathf.Abs(Lanes[i] - Lanes[j]) >= requiredSeparation)
                        pairs.Add(new Vector2Int(i, j));

            if (pairs.Count == 0)
            {
                SpawnObstacle(key1, Lanes[Random.Range(0, Lanes.Length)], z);
                return;
            }
            Vector2Int pair = pairs[Random.Range(0, pairs.Count)];
            SpawnObstacle(key1, Lanes[pair.x], z);
            SpawnObstacle(key2, Lanes[pair.y], z);
        }


        private static float ObstacleHalfWidth(string key)
        {
            switch (key)
            {
                case "building": return 2.1f;
                case "antenna": return 0.8f;
                case "billboard": return 3.45f;
                case "tree": return 1.7f;
                default: return 8.5f;
            }
        }

        private string RandomObstacleKey(bool avoidWire)
        {
            string[] keys = ObstacleFactory.ObstaclePoolKeys;
            string key = keys[Random.Range(0, keys.Length)];
            if (avoidWire && key == "wire")
            {

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

            float x = key == "wire" ? 0f : laneX;
            SpawnFrom(key, x, z);
        }

        private void SpawnWind()
        {


            bool blue = _forceBlueWind || Random.value < 0.55f;
            _forceBlueWind = !blue;
            SpawnFrom(blue ? "wind_blue" : "wind_red", Random.Range(-5f, 5f), SpawnZ + 5f);
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


            Vector3 localPosition = _root.InverseTransformPoint(new Vector3(x, 0f, z));
            GameObject go = pool.Get(localPosition, _root);
            _active.Add(new ActiveSpawn
            {
                Go = go,
                Pool = pool,
                IsObstacle = key == "building" || key == "antenna" ||
                             key == "wire" || key == "billboard" || key == "tree"
            });
        }


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
