using UnityEngine;
using UnityEngine.UI;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // CalibrationUI — overlay de calibragem exibido antes de cada run.
    // Mostra os eixos ao vivo e permite corrigir a orientação do sensor
    // diretamente no dispositivo (Inverter X / Inverter Y / Trocar eixos).
    // ---------------------------------------------------------------------
    public sealed class CalibrationUI : MonoBehaviour
    {
        private GameManager _gm;
        private Text _readout;
        private Text _deviceLabel;

        public void Configure(GameManager gm)
        {
            _gm = gm;
        }

        private void Awake()
        {
            UiFactory.CreateCanvasOn(gameObject, 20);
            BuildUi();
        }

        public void Show()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        private void BuildUi()
        {
            Transform root = transform;
            Vector2 fullMin = Vector2.zero;
            Vector2 fullMax = Vector2.one;
            Vector2 center = new Vector2(0.5f, 0.5f);

            UiFactory.Panel(root, "Dim", fullMin, fullMax, center,
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.72f), true);

            Image panel = UiFactory.Panel(root, "Panel",
                center, center, center, Vector2.zero, new Vector2(940f, 600f),
                UiFactory.ColorPanel, false);

            UiFactory.Label(panel.transform, "Title", "CALIBRAGEM",
                50, TextAnchor.MiddleCenter, UiFactory.ColorText,
                center, center, center, new Vector2(0f, 240f), new Vector2(860f, 60f));

            UiFactory.Label(panel.transform, "Body",
                "Segure o aparelho na posição natural de jogo e\n" +
                "toque em COMEÇAR para capturar a posição neutra.\n\n" +
                "Os eixos ao vivo aparecem abaixo. Se estiverem\n" +
                "trocados, use os botões de correção.",
                28, TextAnchor.MiddleCenter, UiFactory.ColorTextDim,
                center, center, center, new Vector2(0f, 95f), new Vector2(880f, 190f), true);

            _deviceLabel = UiFactory.Label(panel.transform, "DeviceLabel", "",
                26, TextAnchor.MiddleCenter, UiFactory.ColorTextDim,
                center, center, center, new Vector2(0f, -30f), new Vector2(860f, 36f));

            _readout = UiFactory.Label(panel.transform, "AxisReadout",
                "EIXOS  X: +0.00   Y: +0.00",
                32, TextAnchor.MiddleCenter, UiFactory.ColorAmber,
                center, center, center, new Vector2(0f, -80f), new Vector2(860f, 44f));

            // correção de eixos ao vivo no dispositivo:
            // se o tilt sair invertido/trocado, o jogador conserta aqui mesmo
            float fixY = -145f;
            UiFactory.Btn(panel.transform, "InvertXButton", "INVERTER X",
                22, UiFactory.ColorGrey, UiFactory.ColorText,
                center, center, center, new Vector2(-200f, fixY), new Vector2(190f, 50f),
                () => _gm?.FlightInput?.InvertAxis(true));
            UiFactory.Btn(panel.transform, "InvertYButton", "INVERTER Y",
                22, UiFactory.ColorGrey, UiFactory.ColorText,
                center, center, center, new Vector2(0f, fixY), new Vector2(190f, 50f),
                () => _gm?.FlightInput?.InvertAxis(false));
            UiFactory.Btn(panel.transform, "SwapAxesButton", "TROCAR EIXOS",
                22, UiFactory.ColorGrey, UiFactory.ColorText,
                center, center, center, new Vector2(200f, fixY), new Vector2(190f, 50f),
                () => _gm?.FlightInput?.SwapAxes());

            UiFactory.Btn(panel.transform, "StartButton", "COMEÇAR",
                36, UiFactory.ColorGreen, UiFactory.ColorText,
                center, center, center, new Vector2(0f, -225f), new Vector2(560f, 92f),
                () => _gm?.BeginRun());
        }

        private void Update()
        {
            if (_gm == null || _gm.FlightInput == null) return;

            if (_deviceLabel != null)
                _deviceLabel.text = $"ENTRADA:  {_gm.FlightInput.DeviceLabel}";

            if (_readout == null) return;
            Vector2 steer = _gm.FlightInput.Steer;
            _readout.text =
                $"EIXOS  X: {steer.x:+0.00;-0.00}   Y: {steer.y:+0.00;-0.00}";
        }
    }
}
