using UnityEngine;
using UnityEditor;

public static class HierarchyChildCount
{
    public static void DrawChildCount(GameObject obj, ref float xOffset, Rect selectionRect)
    {
        int childCount = obj.transform.childCount;
        if (childCount > 0)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            Rect rect = new Rect(xOffset - 24f, selectionRect.y, 24, 16);
            GUI.Label(rect, childCount.ToString(), style);
            xOffset -= 26f;
        }
    }
}
