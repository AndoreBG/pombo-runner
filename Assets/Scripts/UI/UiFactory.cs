using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // UiFactory — helpers para construir uGUI em runtime (necessário pela
    // estratégia E1: sem prefabs de UI authorados à mão).
    // Canvas de referência: 1600 x 900 (paisagem).
    // ---------------------------------------------------------------------
    public static class UiFactory
    {
        private static Font _font;

        public static readonly Color ColorText = new Color(0.95f, 0.96f, 0.98f);
        public static readonly Color ColorTextDim = new Color(0.72f, 0.76f, 0.82f);
        public static readonly Color ColorPanel = new Color(0.08f, 0.09f, 0.13f, 0.97f);
        public static readonly Color ColorDim = new Color(0f, 0f, 0f, 0.55f);
        public static readonly Color ColorGreen = new Color(0.20f, 0.72f, 0.42f);
        public static readonly Color ColorBlue = new Color(0.22f, 0.52f, 0.90f);
        public static readonly Color ColorGrey = new Color(0.34f, 0.37f, 0.43f);
        public static readonly Color ColorAmber = new Color(1f, 0.80f, 0.25f);
        public static readonly Color ColorRed = new Color(1f, 0.35f, 0.33f);

        public static Font DefaultFont
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_font == null)
                        Debug.LogError("Fonte built-in 'LegacyRuntime.ttf' nao encontrada.");
                }
                return _font;
            }
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        public static Canvas CreateCanvasOn(GameObject host, int sortingOrder)
        {
            var canvas = host.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = host.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            host.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static RectTransform Rect(
            Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return rt;
        }

        public static Image Panel(
            Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, Color color, bool raycast = false)
        {
            RectTransform rt = Rect(parent, name, anchorMin, anchorMax, pivot, anchoredPos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static Text Label(
            Transform parent, string name, string content,
            int fontSize, TextAnchor alignment, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, bool wrap = false)
        {
            RectTransform rt = Rect(parent, name, anchorMin, anchorMax, pivot, anchoredPos, size);
            var text = rt.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button Btn(
            Transform parent, string name, string label,
            int fontSize, Color bg, Color fg,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, Action onClick)
        {
            RectTransform rt = Rect(parent, name, anchorMin, anchorMax, pivot, anchoredPos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = bg;
            img.raycastTarget = true;

            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            if (onClick != null)
                button.onClick.AddListener(() => onClick());

            Label(rt, "Label", label, fontSize, TextAnchor.MiddleCenter, fg,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, false);

            return button;
        }
    }
}
