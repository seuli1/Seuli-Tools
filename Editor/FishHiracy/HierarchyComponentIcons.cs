using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Seulitools
{
    public static class HierarchyComponentIcons
    {
        private static readonly Dictionary<System.Type, Texture2D> iconCache = new Dictionary<System.Type, Texture2D>();
        private static readonly GUIContent tempContent = new GUIContent();
        private static readonly Color disabledTint = new Color(1f, 1f, 1f, 0.4f);

        public static void DrawComponents(GameObject obj, ref float xOffset, Rect selectionRect)
        {
            Component[] components = obj.GetComponents<Component>();

            for (int i = components.Length - 1; i >= 0; i--)
            {
                Component comp = components[i];
                if (comp == null || comp is Transform) continue;

                Texture2D icon = GetComponentIcon(comp);
                if (icon == null) continue;

                bool isToggleable = comp is Behaviour || comp is Collider;
                bool enabledState = true;

                if (comp is Behaviour b) enabledState = b.enabled;
                else if (comp is Collider c) enabledState = c.enabled;

                Rect iconRect = new Rect(xOffset - 16f, selectionRect.y, 16, 16);
                string tooltip = comp.GetType().Name;

                if (comp is SkinnedMeshRenderer smr)
                {
                    tooltip += $"\nVerts: {smr.sharedMesh?.vertexCount ?? 0}, Mats: {smr.sharedMaterials.Length}";
                }

                tempContent.image = icon;
                tempContent.tooltip = tooltip;

                if (isToggleable)
                {
                    GUI.color = enabledState ? Color.white : disabledTint;
                    bool newState = GUI.Toggle(iconRect, enabledState, tempContent, GUIStyle.none);
                    GUI.color = Color.white;

                    if (newState != enabledState)
                    {
                        Undo.RecordObject(comp, "Toggle Component");
                        if (comp is Behaviour b2) b2.enabled = newState;
                        else if (comp is Collider c2) c2.enabled = newState;
                        EditorUtility.SetDirty(comp);
                    }
                }
                else
                {
                    GUI.Label(iconRect, tempContent);
                }

                HierarchyTooltip.DrawTooltip(iconRect, tooltip);
                xOffset -= 18f;
            }
        }

        private static Texture2D GetComponentIcon(Component comp)
        {
            System.Type type = comp.GetType();
            if (!iconCache.TryGetValue(type, out Texture2D icon))
            {
                icon = AssetPreview.GetMiniThumbnail(comp);
                iconCache[type] = icon;
            }
            return icon;
        }
    }
}
