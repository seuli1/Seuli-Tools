using UnityEngine;
using UnityEditor;

namespace Seulitools
{
    public static class HierarchyIndicators
    {
        public static void DrawIndicators(GameObject obj, ref float xOffset, Rect selectionRect)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null)
                {
                    Texture2D missingIcon = EditorGUIUtility.IconContent("console.erroricon.sml").image as Texture2D;
                    if (missingIcon != null)
                    {
                        Rect missingRect = new Rect(xOffset - 16f, selectionRect.y, 16, 16);
                        GUI.Label(missingRect, missingIcon);
                        xOffset -= 18f;
                    }
                }
            }
        }
    }
}
