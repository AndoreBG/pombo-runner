using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // MainMenuBootstrap — composition root da cena MainMenu.
    // Monta o menu em runtime; a cena YAML contém câmera, luz e volume.
    // ---------------------------------------------------------------------
    public sealed class MainMenuBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;

            Camera cam = Camera.main;
            WorldBuilder.SetupGlobals(FindFirstObjectByType<Light>());
            WorldBuilder.SetupCamera(cam);

            var menuGo = new GameObject("MainMenuUI");
            menuGo.AddComponent<MainMenuUI>();

            UiFactory.EnsureEventSystem();
        }
    }
}
