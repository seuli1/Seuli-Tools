using UnityEngine;
using UnityEditor;

public static class HierarchyTooltip
{
    public static void DrawTooltip(Rect rect, string tooltip)
    {
        if (!string.IsNullOrEmpty(tooltip))
            EditorGUI.LabelField(rect, new GUIContent("", tooltip));
    }
}
