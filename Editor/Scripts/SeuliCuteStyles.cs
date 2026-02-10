#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Seulitools
{
    internal static class SeuliCuteStyles
    {
        private static bool initialized;
        private static bool isProSkin;

        private static Texture2D cardTex;
        private static Texture2D toolbarTex;
        private static Texture2D fieldTex;
        private static Texture2D rowTex;
        private static Texture2D rowAltTex;
        private static Texture2D buttonTex;
        private static Texture2D buttonHoverTex;
        private static Texture2D buttonActiveTex;
        private static Texture2D accentTex;
        private static Texture2D accentHoverTex;
        private static Texture2D accentActiveTex;

        public static Color Accent { get; private set; }
        public static Color AccentSoft { get; private set; }
        public static Color TextPrimary { get; private set; }
        public static Color TextMuted { get; private set; }

        public static GUIStyle Card { get; private set; }
        public static GUIStyle Header { get; private set; }
        public static GUIStyle Subheader { get; private set; }
        public static GUIStyle SectionTitle { get; private set; }
        public static GUIStyle Toolbar { get; private set; }
        public static GUIStyle ToolbarButton { get; private set; }
        public static GUIStyle ToolbarPrimaryButton { get; private set; }
        public static GUIStyle TextField { get; private set; }
        public static GUIStyle SearchField { get; private set; }
        public static GUIStyle PrimaryButton { get; private set; }
        public static GUIStyle SecondaryButton { get; private set; }
        public static GUIStyle MiniToggle { get; private set; }
        public static GUIStyle Row { get; private set; }
        public static GUIStyle RowAlt { get; private set; }
        public static GUIStyle RowButton { get; private set; }
        public static GUIStyle Pill { get; private set; }
        public static GUIStyle MutedLabel { get; private set; }
        public static GUIStyle Footer { get; private set; }

        public static void Ensure()
        {
            bool currentPro = EditorGUIUtility.isProSkin;
            if (initialized && currentPro == isProSkin)
            {
                return;
            }

            isProSkin = currentPro;
            initialized = true;

            Accent = Color.white;
            AccentSoft = Color.white;
            TextPrimary = EditorStyles.label.normal.textColor;
            TextMuted = EditorStyles.miniLabel.normal.textColor;

            Card = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(6, 6, 6, 6)
            };

            Header = new GUIStyle(EditorStyles.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            Subheader = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11
            };

            SectionTitle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            Toolbar = new GUIStyle(EditorStyles.toolbar)
            {
                padding = new RectOffset(8, 8, 6, 6)
            };
            Toolbar.fixedHeight = 28f;

            ToolbarButton = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 20f,
                fontSize = 10,
                padding = new RectOffset(8, 8, 2, 2)
            };

            ToolbarPrimaryButton = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 20f,
                fontStyle = FontStyle.Bold,
                fontSize = 10,
                padding = new RectOffset(8, 8, 2, 2)
            };

            TextField = new GUIStyle(EditorStyles.textField)
            {
            };

            SearchField = new GUIStyle(EditorStyles.textField)
            {
                padding = new RectOffset(8, 8, 4, 4)
            };

            PrimaryButton = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 24f,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(10, 10, 4, 4)
            };

            SecondaryButton = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 24f,
                padding = new RectOffset(10, 10, 4, 4)
            };

            MiniToggle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 20f,
                fontSize = 9,
                padding = new RectOffset(6, 6, 2, 2)
            };

            Row = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 6, 6)
            };

            RowAlt = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 6, 6)
            };

            RowButton = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            Pill = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 2, 2)
            };

            MutedLabel = new GUIStyle(EditorStyles.label);
            Footer = new GUIStyle(EditorStyles.centeredGreyMiniLabel);

        }

        public static void ApplyToggleColor(bool isOn)
        {
            GUI.backgroundColor = Color.white;
        }

        public static void ResetColors()
        {
            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;
        }

        private static Texture2D MakeTex(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }
    }
}
#endif
