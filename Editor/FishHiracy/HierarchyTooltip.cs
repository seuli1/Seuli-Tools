using UnityEngine;
using UnityEditor;

namespace Seulitools
{
    public static class HierarchyTooltip
    {
        public static void DrawTooltip(Rect rect, string tooltip)
        {
            if (!string.IsNullOrEmpty(tooltip))
            {
                EditorGUI.LabelField(rect, new GUIContent("", tooltip));
            }
        }
    }
}
