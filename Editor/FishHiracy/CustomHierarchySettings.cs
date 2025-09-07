using UnityEngine;
using UnityEditor;

public class CustomHierarchySettings : EditorWindow
{
    private bool showComponentIcons;
    private bool showActiveToggle;
    private bool showIndicators;
    private bool showChildCount;

    [MenuItem("SeuliTools/Fish Hierarchy/Settings")]
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
    EditorGUILayout.Space();
    GUILayout.Label("🐟 Fish Hierarchy Settings", EditorStyles.boldLabel);
    EditorGUILayout.Space();

    GUI.backgroundColor = new Color(0.9f, 0.95f, 1f);
    EditorGUILayout.BeginVertical("box");
    GUI.backgroundColor = Color.white;

    GUILayout.Label("Display Options", EditorStyles.boldLabel);
    EditorGUILayout.Space(5);

    showComponentIcons = EditorGUILayout.ToggleLeft(
        new GUIContent(" Show Component Icons", "Displays small icons for components in the hierarchy."),
        showComponentIcons,
        EditorStyles.label
    );

    showActiveToggle = EditorGUILayout.ToggleLeft(
        new GUIContent(" Show Active Toggle", "Displays a checkbox to toggle GameObject active state in hierarchy."),
        showActiveToggle,
        EditorStyles.label
    );

    showIndicators = EditorGUILayout.ToggleLeft(
        new GUIContent(" Show Error Indicators", "Shows error/warning indicators next to GameObjects with issues."),
        showIndicators,
        EditorStyles.label
    );

    showChildCount = EditorGUILayout.ToggleLeft(
        new GUIContent(" Show Child Count", "Displays the number of child objects next to parent GameObjects."),
        showChildCount,
        EditorStyles.label
    );

    EditorGUILayout.EndVertical();
    EditorGUILayout.Space(10);

    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();

    GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
    if (GUILayout.Button(" Save Settings", GUILayout.Height(30), GUILayout.Width(150)))
    {
        EditorPrefs.SetBool("CH_ShowComponentIcons", showComponentIcons);
        EditorPrefs.SetBool("CH_ShowActiveToggle", showActiveToggle);
        EditorPrefs.SetBool("CH_ShowIndicators", showIndicators);
        EditorPrefs.SetBool("CH_ShowChildCount", showChildCount);
        Close();
    }

    GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
    if (GUILayout.Button(" Reset Defaults", GUILayout.Height(30), GUILayout.Width(150)))
    {
        showComponentIcons = true;
        showActiveToggle = true;
        showIndicators = true;
        showChildCount = true;
    }

    GUI.backgroundColor = Color.white;
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.Space(10);

    GUILayout.Label("SeuliTools © 2025", EditorStyles.centeredGreyMiniLabel);
}

    public static bool ShowComponentIcons => EditorPrefs.GetBool("CH_ShowComponentIcons", true);
    public static bool ShowActiveToggle => EditorPrefs.GetBool("CH_ShowActiveToggle", true);
    public static bool ShowIndicators => EditorPrefs.GetBool("CH_ShowIndicators", true);
    public static bool ShowChildCount => EditorPrefs.GetBool("CH_ShowChildCount", true);
}
