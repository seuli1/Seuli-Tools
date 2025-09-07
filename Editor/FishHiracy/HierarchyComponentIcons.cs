using UnityEngine;
using UnityEditor;

public static class HierarchyComponentIcons
{
    public static void DrawComponents(GameObject obj, ref float xOffset, Rect selectionRect)
    {
        Component[] components = obj.GetComponents<Component>();

        for (int i = components.Length - 1; i >= 0; i--)
        {
            Component comp = components[i];
            if (comp == null || comp is Transform) continue;

            Texture2D icon = AssetPreview.GetMiniThumbnail(comp);
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

            GUIContent content = new GUIContent(icon, tooltip);

            if (isToggleable)
            {
                GUI.color = enabledState ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                bool newState = GUI.Toggle(iconRect, enabledState, content, GUIStyle.none);
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
                GUI.Label(iconRect, content);
            }

            HierarchyTooltip.DrawTooltip(iconRect, tooltip);
            xOffset -= 18f;
        }
    }
}
