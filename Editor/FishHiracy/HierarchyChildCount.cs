using UnityEngine;
using UnityEditor;

namespace Seulitools
{
    public static class HierarchyChildCount
    {
        private static GUIStyle childCountStyle;

        public static void DrawChildCount(GameObject obj, ref float xOffset, Rect selectionRect)
        {
            int childCount = obj.transform.childCount;
            if (childCount > 0)
            {
                if (childCountStyle == null)
                {
                    childCountStyle = new GUIStyle(EditorStyles.miniLabel);
                    childCountStyle.normal.textColor = Color.white;
                    childCountStyle.alignment = TextAnchor.MiddleCenter;
                }
                Rect rect = new Rect(xOffset - 24f, selectionRect.y, 24, 16);
                GUI.Label(rect, childCount.ToString(), childCountStyle);
                xOffset -= 26f;
            }
        }
    }
}
