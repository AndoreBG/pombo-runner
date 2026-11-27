using UnityEngine;
using UnityEngine.SceneManagement;

namespace PigeonRooftopRush
{
    public enum GameState
    {
        Boot,
        Calibrating,
        Playing,
        GameOver
    }


    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;

        public IFlightInput FlightInput { get; private set; }
        public PigeonFlightController Flight { get; private set; }
        public EnergySystem Energy { get; private set; }
        public SpawnManager Spawner { get; private set; }
        public ScoreManager Score { get; private set; }
        public DifficultyManager Difficulty { get; private set; }
        public HUD Hud { get; private set; }
        public CalibrationUI Calibration { get; private set; }
        public CollisionHandler Collisions { get; private set; }

        private Transform _worldRoot;
        private bool _pendingCrash;
        private CrashReason _pendingCrashReason;
        private Transform _cameraTransform;
        private Transform _playerTransform;
        private Transform _shadowTransform;
        private Material _shadowMaterial;
        private int _runIndex;
        private Camera _cameraComponent;

        public void Setup(
            IFlightInput input,
            PigeonFlightController flight,
            EnergySystem energy,
            SpawnManager spawner,
            ScoreManager score,
            DifficultyManager difficulty,
            HUD hud,
            CalibrationUI calibration,
            CollisionHandler collisions,
            Transform worldRoot,
            Transform cameraTransform,
            Transform playerTransform,
            Transform shadowTransform,
            Material shadowMaterial)
        {
            FlightInput = input;
            Flight = flight;
            Energy = energy;
            Spawner = spawner;
            Score = score;
            Difficulty = difficulty;
            Hud = hud;
            Calibration = calibration;
            Collisions = collisions;
            _worldRoot = worldRoot;
            _cameraTransform = cameraTransform;
            _cameraComponent = cameraTransform != null
                ? cameraTransform.GetComponent<Camera>()
                : null;
            _playerTransform = playerTransform;
            _shadowTransform = shadowTransform;
            _shadowMaterial = shadowMaterial;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 1f / 60f;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CrashEvent>(OnCrash);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CrashEvent>(OnCrash);
        }

        private void OnDestroy()
        {
            if (FlightInput is System.IDisposable disposable) disposable.Dispose();
            if (Instance == this) Instance = null;
        }

        private void OnCrash(CrashEvent evt)
        {

            _pendingCrash = true;
            _pendingCrashReason = evt.Reason;
        }


        public void StartCalibration()
        {
            State = GameState.Calibrating;
            FlightInput.Calibrate();
            Flight.ControlsEnabled = false;
            Collisions.Running = false;
            Calibration.Show();
        }

        public void BeginRun()
        {
            if (State != GameState.Calibrating) return;


            FlightInput.Calibrate();

            State = GameState.Playing;
            _pendingCrash = false;
            _runIndex++;
            Score.BeginRun();
            Difficulty.BeginRun();
            Spawner.BeginRun();
            Flight.ResetRun();
            Energy.Reset();
            Flight.ControlsEnabled = true;
            Collisions.Running = true;
            Calibration.Hide();
            Hud.ShowPlaying();
            EventBus.Raise(new RunStartedEvent { RunIndex = _runIndex });
        }

        public void EndRun(CrashReason reason)
        {
            if (State != GameState.Playing) return;

            State = GameState.GameOver;
            Flight.ControlsEnabled = false;
            Collisions.Running = false;
            Spawner.Stop();

            bool isNewBest = Score.SubmitBest();
            Hud.ShowGameOver(reason, Score.Score, Score.Best, isNewBest);
            EventBus.Raise(new RunEndedEvent
            {
                Reason = reason,
                Score = Score.Score,
                IsNewBest = isNewBest
            });
        }

        public void Restart()
        {
            SceneManager.LoadScene("Game");
        }

        public void GoToMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }


        private void Update()
        {
            if (_pendingCrash && State == GameState.Playing)
            {
                _pendingCrash = false;
                EndRun(_pendingCrashReason);
            }

            switch (State)
            {
                case GameState.Playing:
                    TickPlaying(Time.deltaTime);
                    break;
                case GameState.Calibrating:

                    FlightInput.Tick();
                    break;
            }

            UpdateCamera(Time.deltaTime);
            UpdateShadow();
        }

        private void TickPlaying(float dt)
        {
            FlightInput.Tick();

            float speed = Difficulty.Tick(dt);
            Spawner.Tick(dt);
            Score.Tick(dt, speed);
            Energy.Tick(dt, Flight.IsGliding);

            if (_worldRoot != null)
                _worldRoot.position += Vector3.back * speed * dt;

            if (Flight.GroundTouched)
                EndRun(CrashReason.Ground);
        }

        private void UpdateCamera(float dt)
        {
            if (_cameraTransform == null || _playerTransform == null) return;

            var desired = new Vector3(0f, 6f, -10f);


            bool turbulence = State == GameState.Playing &&
                              Energy != null && Energy.InWind &&
                              Energy.Wind == WindType.Turbulence;
            if (turbulence)
                desired += (Vector3)(Random.insideUnitCircle * 0.11f);

            float t = 1f - Mathf.Exp(-8f * dt);
            _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, desired, t);

            if (_cameraComponent != null)
            {
                float targetFov = 65f;
                if (State == GameState.Playing && Difficulty != null)
                    targetFov = Mathf.Lerp(62f, 74f,
                        Mathf.Clamp01((Difficulty.Speed - 9f) / 15f));
                _cameraComponent.fieldOfView =
                    Mathf.Lerp(_cameraComponent.fieldOfView, targetFov, t);
            }
        }

        private void UpdateShadow()
        {
            if (_shadowTransform == null || _playerTransform == null) return;

            float y = _playerTransform.position.y;
            _shadowTransform.position = new Vector3(_playerTransform.position.x, 0.03f, 0f);
            float s = 0.9f + y * 0.055f;
            _shadowTransform.localScale = new Vector3(s, s, s);

            if (_shadowMaterial != null)
            {
                float a = Mathf.Lerp(0.30f, 0.10f, Mathf.Clamp01(y / 22f));
                _shadowMaterial.SetColor("_BaseColor", new Color(0f, 0f, 0f, a));
            }
        }
    }
}
