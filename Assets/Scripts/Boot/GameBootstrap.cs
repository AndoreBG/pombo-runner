using UnityEngine;

namespace PigeonRooftopRush
{


    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 1f / 60f;


            Camera cam = Camera.main;
            Light sun = FindFirstObjectByType<Light>();
            WorldBuilder.SetupGlobals(sun);
            WorldBuilder.SetupCamera(cam);


            Transform worldRoot = WorldBuilder.BuildWorldRoot();
            WorldBuilder.BuildGround();
            Transform playerShadow = WorldBuilder.BuildBlobShadow(out Material shadowMat);


            var systemsGo = new GameObject("Systems");
            systemsGo.SetActive(false);
            GameManager gm = systemsGo.AddComponent<GameManager>();

            IFlightInput input = Application.isMobilePlatform
                ? (IFlightInput)new MobileTiltInput()
                : new EditorInput();

            var energy = new EnergySystem();
            var score = new ScoreManager();
            var difficulty = new DifficultyManager();
            var spawner = new SpawnManager();
            spawner.Init(worldRoot, difficulty, score);


            PigeonFlightController flight =
                PigeonBuilder.Build(new Vector3(0f, 6f, 0f));
            flight.InputSource = input;
            flight.FlightCamera = cam;
            flight.Energy = energy;
            CollisionHandler collisions = flight.GetComponent<CollisionHandler>();
            collisions.Energy = energy;


            var hudGo = new GameObject("HUD");
            hudGo.SetActive(false);
            HUD hud = hudGo.AddComponent<HUD>();

            var calGo = new GameObject("CalibrationUI");
            calGo.SetActive(false);
            CalibrationUI calibration = calGo.AddComponent<CalibrationUI>();

            hud.Configure(gm);
            calibration.Configure(gm);

            gm.Setup(
                input, flight, energy, spawner, score, difficulty,
                hud, calibration, collisions,
                worldRoot,
                cam != null ? cam.transform : null,
                flight.transform,
                playerShadow, shadowMat);


            systemsGo.SetActive(true);
            hudGo.SetActive(true);
            calGo.SetActive(true);

            UiFactory.EnsureEventSystem();


            gm.StartCalibration();
        }
    }
}
