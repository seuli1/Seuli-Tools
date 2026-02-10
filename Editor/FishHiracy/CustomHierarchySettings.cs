using UnityEngine;
using UnityEditor;

namespace Seulitools
{
    public class CustomHierarchySettings : EditorWindow
    {
        private bool showComponentIcons;
        private bool showActiveToggle;
        private bool showIndicators;
        private bool showChildCount;

        [MenuItem("SeuliTools/Fish Hierarchy/Settings", false, 1)]
        public static void ShowWindow() => GetWindow<CustomHierarchySettings>("Hierarchy Settings");

        private void OnEnable()
        {
            showComponentIcons = EditorPrefs.GetBool("CH_ShowComponentIcons", true);
            showActiveToggle = EditorPrefs.GetBool("CH_ShowActiveToggle", true);
            showIndicators = EditorPrefs.GetBool("CH_ShowIndicators", true);
            showChildCount = EditorPrefs.GetBool("CH_ShowChildCount", true);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            GUILayout.Label("Fish Hierarchy Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Display", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            showComponentIcons = EditorGUILayout.ToggleLeft(
                new GUIContent("Show Component Icons", "Displays small icons for components in the hierarchy."),
                showComponentIcons
            );

            showActiveToggle = EditorGUILayout.ToggleLeft(
                new GUIContent("Show Active Toggle", "Displays a checkbox to toggle GameObject active state in hierarchy."),
                showActiveToggle
            );

            showIndicators = EditorGUILayout.ToggleLeft(
                new GUIContent("Show Error Indicators", "Shows error/warning indicators next to GameObjects with issues."),
                showIndicators
            );

            showChildCount = EditorGUILayout.ToggleLeft(
                new GUIContent("Show Child Count", "Displays the number of child objects next to parent GameObjects."),
                showChildCount
            );

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save Settings", GUILayout.Width(150)))
            {
                EditorPrefs.SetBool("CH_ShowComponentIcons", showComponentIcons);
                EditorPrefs.SetBool("CH_ShowActiveToggle", showActiveToggle);
                EditorPrefs.SetBool("CH_ShowIndicators", showIndicators);
                EditorPrefs.SetBool("CH_ShowChildCount", showChildCount);
                Close();
            }

            if (GUILayout.Button("Reset Defaults", GUILayout.Width(150)))
            {
                showComponentIcons = true;
                showActiveToggle = true;
                showIndicators = true;
                showChildCount = true;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8f);

            GUILayout.Label("SeuliTools 2025", EditorStyles.centeredGreyMiniLabel);
        }

        public static bool ShowComponentIcons => EditorPrefs.GetBool("CH_ShowComponentIcons", true);
        public static bool ShowActiveToggle => EditorPrefs.GetBool("CH_ShowActiveToggle", true);
        public static bool ShowIndicators => EditorPrefs.GetBool("CH_ShowIndicators", true);
        public static bool ShowChildCount => EditorPrefs.GetBool("CH_ShowChildCount", true);
    }
}
