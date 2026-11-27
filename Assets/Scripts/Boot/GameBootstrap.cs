using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // GameBootstrap — composition root da cena Game.
    // Monta TODO o jogo em runtime (plano, E1): mundo, jogador, sistemas,
    // pools, HUD e calibragem. A cena YAML contém apenas câmera, luz,
    // volume e este objeto.
    // ---------------------------------------------------------------------
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 1f / 60f;

            // ---- cena: câmera + luz + globals dos shaders ----
            Camera cam = Camera.main;
            Light sun = FindFirstObjectByType<Light>();
            WorldBuilder.SetupGlobals(sun);
            WorldBuilder.SetupCamera(cam);

            // ---- mundo ----
            Transform worldRoot = WorldBuilder.BuildWorldRoot();
            WorldBuilder.BuildGround();
            Transform playerShadow = WorldBuilder.BuildBlobShadow(out Material shadowMat);

            // ---- sistemas (GO inativo: Awake só roda após Setup) ----
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

            // ---- jogador ----
            PigeonFlightController flight =
                PigeonBuilder.Build(new Vector3(0f, 6f, 0f));
            flight.InputSource = input;
            flight.Energy = energy;
            CollisionHandler collisions = flight.GetComponent<CollisionHandler>();
            collisions.Energy = energy;

            // ---- UI (GOs inativos) ----
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

            // ---- ativa (Awakes rodam com as referências prontas) ----
            systemsGo.SetActive(true);
            hudGo.SetActive(true);
            calGo.SetActive(true);

            UiFactory.EnsureEventSystem();

            // ---- entra no estado inicial: calibragem ----
            gm.StartCalibration();
        }
    }
}
