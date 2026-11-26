using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // MainMenuUI — menu principal (Jogar / Como jogar / Sair) + recorde.
    // ADIÇÃO fora da lista do PDF: a estrutura de 2 cenas exige um menu.
    // ---------------------------------------------------------------------
    public sealed class MainMenuUI : MonoBehaviour
    {
        private GameObject _helpPanel;

        private void Awake()
        {
            UiFactory.CreateCanvasOn(gameObject, 10);
            UiFactory.EnsureEventSystem();
            BuildUi();
        }

        private void BuildUi()
        {
            Transform root = transform;
            Vector2 fullMin = Vector2.zero;
            Vector2 fullMax = Vector2.one;
            Vector2 center = new Vector2(0.5f, 0.5f);

            // faixa inferior "chão" para dar contexto visual
            UiFactory.Panel(root, "Ground", fullMin,
                new Vector2(1f, 0.18f), new Vector2(0.5f, 0f),
                Vector2.zero, Vector2.zero, new Color(0.16f, 0.17f, 0.20f));

            // título (sombra + título)
            UiFactory.Label(root, "TitleShadow", "PIGEON ROOFTOP RUSH",
                84, TextAnchor.MiddleCenter, new Color(0f, 0f, 0f, 0.35f),
                center, center, center, new Vector2(4f, 266f), new Vector2(1500f, 110f));
            UiFactory.Label(root, "Title", "PIGEON ROOFTOP RUSH",
                84, TextAnchor.MiddleCenter, UiFactory.ColorText,
                center, center, center, new Vector2(0f, 270f), new Vector2(1500f, 110f));

            UiFactory.Label(root, "Subtitle",
                "Um pombo, uma cidade e a vontade de chegar longe.",
                32, TextAnchor.MiddleCenter, UiFactory.ColorTextDim,
                center, center, center, new Vector2(0f, 188f), new Vector2(1200f, 46f));

            UiFactory.Label(root, "BestLabel",
                $"RECORDE  {PlayerPrefs.GetInt("PRR_BestScore", 0)}",
                36, TextAnchor.MiddleCenter, UiFactory.ColorAmber,
                center, center, center, new Vector2(0f, 118f), new Vector2(800f, 50f));

            UiFactory.Btn(root, "PlayButton", "JOGAR",
                40, UiFactory.ColorGreen, UiFactory.ColorText,
                center, center, center, new Vector2(0f, 10f), new Vector2(620f, 96f),
                () => SceneManager.LoadScene("Game"));

            UiFactory.Btn(root, "HelpButton", "COMO JOGAR",
                36, UiFactory.ColorBlue, UiFactory.ColorText,
                center, center, center, new Vector2(0f, -110f), new Vector2(620f, 96f),
                ToggleHelp);

            UiFactory.Btn(root, "QuitButton", "SAIR",
                36, UiFactory.ColorGrey, UiFactory.ColorText,
                center, center, center, new Vector2(0f, -230f), new Vector2(620f, 96f),
                Quit);

            UiFactory.Label(root, "Version",
                "MVP · Unity 6 · Android",
                22, TextAnchor.LowerRight, UiFactory.ColorTextDim,
                fullMin, fullMax, new Vector2(1f, 0f),
                new Vector2(-30f, 24f), new Vector2(400f, 34f));

            BuildHelpPanel(root, center, fullMin, fullMax);
        }

        private void BuildHelpPanel(Transform root, Vector2 center, Vector2 fullMin, Vector2 fullMax)
        {
            _helpPanel = new GameObject("HelpPanel", typeof(RectTransform));
            var rt = _helpPanel.GetComponent<RectTransform>();
            rt.SetParent(root, false);
            rt.anchorMin = fullMin;
            rt.anchorMax = fullMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            UiFactory.Panel(rt, "Dim", Vector2.zero, Vector2.one, center,
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.7f), true);

            Image panel = UiFactory.Panel(rt, "Panel",
                center, center, center, Vector2.zero, new Vector2(1100f, 640f),
                UiFactory.ColorPanel, false);

            UiFactory.Label(panel.transform, "Title", "COMO JOGAR",
                48, TextAnchor.MiddleCenter, UiFactory.ColorText,
                center, center, center, new Vector2(0f, 248f), new Vector2(1000f, 60f));

            UiFactory.Label(panel.transform, "Body",
                "INCLINE O APARELHO  para virar, subir e mergulhar.\n" +
                "TOQUE NA TELA  para bater asas.  SEGURE  para planar.\n\n" +
                "ENERGIA:  voar custa energia e bater asas custa mais.\n" +
                "Planar economiza. A corrente AZUL recupera energia;\n" +
                "a VERMELHA drena e sacode o voo.\n\n" +
                "OBSTÁCULOS:  prédios, antenas, fios elétricos,\n" +
                "outdoors e árvores. Desvie e vá o mais longe possível!\n\n" +
                "No editor: WASD/setas inclinam, Espaço bate asas,\n" +
                "Shift plana.",
                28, TextAnchor.UpperCenter, UiFactory.ColorTextDim,
                center, center, center, new Vector2(0f, -10f), new Vector2(1000f, 440f), true);

            UiFactory.Btn(panel.transform, "CloseButton", "FECHAR",
                32, UiFactory.ColorGrey, UiFactory.ColorText,
                center, center, center, new Vector2(0f, -250f), new Vector2(460f, 84f),
                ToggleHelp);

            _helpPanel.SetActive(false);
        }

        private void ToggleHelp()
        {
            if (_helpPanel != null)
                _helpPanel.SetActive(!_helpPanel.activeSelf);
        }

        private void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
