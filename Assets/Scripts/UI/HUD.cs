using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // HUD — score, recorde, velocidade, barra de energia e overlay de
    // game over. Assina eventos pelo EventBus (OnEnable/OnDisable).
    // ---------------------------------------------------------------------
    public sealed class HUD : MonoBehaviour
    {
        private GameManager _gm;

        private Text _scoreText;
        private Text _bestText;
        private Text _speedText;
        private Text _energyText;
        private Text _emptyText;
        private Image _energyFill;
        private GameObject _gameOverRoot;
        private Text _goReason;
        private Text _goScore;
        private Text _goBest;
        private GameObject _goNewBest;
        private int _lastSpeed = int.MinValue;
        private Coroutine _emptyFlash;

        public void Configure(GameManager gm)
        {
            _gm = gm;
        }

        private void Awake()
        {
            UiFactory.CreateCanvasOn(gameObject, 10);
            BuildUi();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<EnergyChangedEvent>(OnEnergyChanged);
            EventBus.Subscribe<FlapBlockedEvent>(OnFlapBlocked);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<EnergyChangedEvent>(OnEnergyChanged);
            EventBus.Unsubscribe<FlapBlockedEvent>(OnFlapBlocked);
        }

        // ------------------------------------------------------------ build

        private void BuildUi()
        {
            Transform root = transform;
            Vector2 fullMin = Vector2.zero;
            Vector2 fullMax = Vector2.one;
            Vector2 center = new Vector2(0.5f, 0.5f);

            // ---- canto superior esquerdo: pontos/recorde ----
            _scoreText = UiFactory.Label(root, "ScoreText", "PONTOS  0",
                44, TextAnchor.UpperLeft, UiFactory.ColorText,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -24f), new Vector2(600f, 60f));

            _bestText = UiFactory.Label(root, "BestText", "RECORDE  0",
                30, TextAnchor.UpperLeft, UiFactory.ColorTextDim,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -88f), new Vector2(600f, 44f));

            // ---- canto superior direito: velocidade ----
            _speedText = UiFactory.Label(root, "SpeedText", "0 m/s",
                34, TextAnchor.UpperRight, UiFactory.ColorText,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-40f, -24f), new Vector2(400f, 50f));

            // ---- rodapé: barra de energia ----
            Image barBg = UiFactory.Panel(root, "EnergyBackground",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 26f), new Vector2(560f, 36f),
                new Color(0f, 0f, 0f, 0.5f));

            _energyFill = UiFactory.Panel(barBg.transform, "EnergyFill",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, UiFactory.ColorAmber);
            _energyFill.type = Image.Type.Filled;
            _energyFill.fillMethod = Image.FillMethod.Horizontal;
            _energyFill.fillOrigin = 0;
            _energyFill.fillAmount = 1f;

            _energyText = UiFactory.Label(barBg.transform, "EnergyLabel", "ENERGIA  100%",
                26, TextAnchor.MiddleCenter, UiFactory.ColorText,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 56f), new Vector2(560f, 32f));

            _emptyText = UiFactory.Label(root, "EmptyEnergyLabel", "SEM ENERGIA!",
                28, TextAnchor.MiddleCenter, UiFactory.ColorRed,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 118f), new Vector2(560f, 36f));
            _emptyText.gameObject.SetActive(false);

            // ---- overlay de game over ----
            _gameOverRoot = new GameObject("GameOverOverlay", typeof(RectTransform));
            var goRt = _gameOverRoot.GetComponent<RectTransform>();
            goRt.SetParent(root, false);
            goRt.anchorMin = fullMin;
            goRt.anchorMax = fullMax;
            goRt.offsetMin = Vector2.zero;
            goRt.offsetMax = Vector2.zero;

            UiFactory.Panel(goRt, "Dim", fullMin, fullMax, center,
                Vector2.zero, Vector2.zero, UiFactory.ColorDim, true);

            Image panel = UiFactory.Panel(goRt, "Panel",
                center, center, center, Vector2.zero, new Vector2(720f, 540f),
                UiFactory.ColorPanel, false);

            UiFactory.Label(panel.transform, "Title", "FIM DE JOGO",
                52, TextAnchor.MiddleCenter, UiFactory.ColorText,
                center, center, center, new Vector2(0f, 185f), new Vector2(660f, 70f));

            _goReason = UiFactory.Label(panel.transform, "Reason", "",
                30, TextAnchor.MiddleCenter, UiFactory.ColorTextDim,
                center, center, center, new Vector2(0f, 122f), new Vector2(660f, 46f));

            _goScore = UiFactory.Label(panel.transform, "Score", "PONTOS  0",
                42, TextAnchor.MiddleCenter, UiFactory.ColorText,
                center, center, center, new Vector2(0f, 58f), new Vector2(660f, 56f));

            _goBest = UiFactory.Label(panel.transform, "Best", "RECORDE  0",
                30, TextAnchor.MiddleCenter, UiFactory.ColorTextDim,
                center, center, center, new Vector2(0f, 4f), new Vector2(660f, 44f));

            _goNewBest = UiFactory.Label(panel.transform, "NewBest", "NOVO RECORDE!",
                32, TextAnchor.MiddleCenter, UiFactory.ColorGreen,
                center, center, center, new Vector2(0f, -44f), new Vector2(660f, 44f));
            _goNewBest.gameObject.SetActive(false);

            UiFactory.Btn(panel.transform, "RetryButton", "JOGAR NOVAMENTE",
                32, UiFactory.ColorGreen, UiFactory.ColorText,
                center, center, center, new Vector2(0f, -125f), new Vector2(540f, 82f),
                () => _gm?.Restart());

            UiFactory.Btn(panel.transform, "MenuButton", "MENU PRINCIPAL",
                32, UiFactory.ColorGrey, UiFactory.ColorText,
                center, center, center, new Vector2(0f, -220f), new Vector2(540f, 82f),
                () => _gm?.GoToMenu());

            _gameOverRoot.SetActive(false);

            // estado inicial
            if (_gm != null && _gm.Score != null)
                _bestText.text = $"RECORDE  {_gm.Score.Best}";
        }

        // ------------------------------------------------------------ API

        public void ShowPlaying()
        {
            if (_gameOverRoot != null) _gameOverRoot.SetActive(false);
        }

        public void ShowGameOver(CrashReason reason, int score, int best, bool isNewBest)
        {
            _goReason.text = reason == CrashReason.Obstacle ? "Você bateu!" : "Você caiu!";
            _goScore.text = $"PONTOS  {score}";
            _goBest.text = $"RECORDE  {best}";
            _goNewBest.SetActive(isNewBest);
            _gameOverRoot.SetActive(true);
        }

        // --------------------------------------------------------- eventos

        private void OnScoreChanged(ScoreChangedEvent e)
        {
            _scoreText.text = $"PONTOS  {e.Score}";
            _bestText.text = $"RECORDE  {Mathf.Max(_gm != null && _gm.Score != null ? _gm.Score.Best : 0, e.Score)}";
        }

        private void OnEnergyChanged(EnergyChangedEvent e)
        {
            if (_energyFill != null) _energyFill.fillAmount = e.Percent / 100f;
            if (_energyText != null) _energyText.text = $"ENERGIA  {e.Percent}%";
            if (_emptyText != null && e.Percent > 0) _emptyText.SetActive(false);
        }

        private void OnFlapBlocked(FlapBlockedEvent e)
        {
            if (_emptyText == null) return;
            if (_emptyFlash != null) StopCoroutine(_emptyFlash);
            _emptyFlash = StartCoroutine(FlashEmpty());
        }

        private IEnumerator FlashEmpty()
        {
            _emptyText.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            _emptyText.SetActive(false);
            _emptyFlash = null;
        }

        private void Update()
        {
            if (_gm == null || _speedText == null) return;
            if (_gm.State != GameState.Playing || _gm.Difficulty == null) return;

            int speed = Mathf.RoundToInt(_gm.Difficulty.Speed);
            if (speed == _lastSpeed) return;
            _lastSpeed = speed;
            _speedText.text = $"{speed} m/s";
        }
    }
}
