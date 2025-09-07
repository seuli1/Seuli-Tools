using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public static class CustomHierarchy
{
    private static readonly float iconSize = 16f;
    private static readonly float iconSpacing = 2f;
    private static readonly Dictionary<System.Type, Texture2D> componentIconCache = new Dictionary<System.Type, Texture2D>();

    static CustomHierarchy()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        float xOffset = selectionRect.xMax;

        if (CustomHierarchySettings.ShowActiveToggle)
        {
            xOffset -= iconSize;
            Rect eyeRect = new Rect(xOffset, selectionRect.y, iconSize, iconSize);
            Texture2D eyeIcon = EditorGUIUtility.IconContent(obj.activeSelf ? "d_scenevis_visible_hover" : "d_scenevis_hidden_hover").image as Texture2D;

            if (GUI.Button(eyeRect, eyeIcon, GUIStyle.none))
            {
                Undo.RecordObject(obj, "Toggle Active");
                obj.SetActive(!obj.activeSelf);
                EditorUtility.SetDirty(obj);
            }
        }

        if (PrefabUtility.IsAnyPrefabInstanceRoot(obj))
        {
            EditorGUI.DrawRect(selectionRect, new Color(0.2f, 0.5f, 1f, 0.15f));
        }

        Component[] components = obj.GetComponents<Component>();

        for (int i = components.Length - 1; i >= 0; i--)
        {
            Component comp = components[i];

            if (comp == null && CustomHierarchySettings.ShowIndicators)
            {
                Texture2D missingIcon = EditorGUIUtility.IconContent("console.erroricon.sml").image as Texture2D;
                if (missingIcon != null)
                {
                    xOffset -= iconSize + iconSpacing;
                    Rect rect = new Rect(xOffset, selectionRect.y, iconSize, iconSize);
                    GUI.Label(rect, missingIcon, new GUIStyle() { alignment = TextAnchor.MiddleCenter });
                }
                continue;
            }

            if (comp != null && !(comp is Transform) && CustomHierarchySettings.ShowComponentIcons)
            {
                xOffset -= iconSize + iconSpacing;
                Rect rect = new Rect(xOffset, selectionRect.y, iconSize, iconSize);

                Texture2D icon = GetComponentIcon(comp);
                string tooltip = comp.GetType().Name;

                if (comp is SkinnedMeshRenderer smr)
                    tooltip += $"\nVerts: {smr.sharedMesh?.vertexCount ?? 0}, Mats: {smr.sharedMaterials.Length}";

                GUIContent content = new GUIContent(icon, tooltip);

                bool isToggleable = comp is Behaviour || comp is Collider;
                if (isToggleable)
                {
                    bool enabledState = comp is Behaviour b ? b.enabled : (comp as Collider).enabled;
                    GUI.color = enabledState ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                    bool newState = GUI.Toggle(rect, enabledState, content, GUIStyle.none);
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
                    GUI.Label(rect, content);
                }
            }
        }

        if (CustomHierarchySettings.ShowChildCount && obj.transform.childCount > 0)
        {
            xOffset -= 24f + iconSpacing;
            Rect rect = new Rect(xOffset, selectionRect.y, 24, 16);
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            GUI.Label(rect, obj.transform.childCount.ToString(), style);
        }
    }

    private static Texture2D GetComponentIcon(Component comp)
    {
        System.Type type = comp.GetType();
        if (!componentIconCache.TryGetValue(type, out Texture2D icon))
        {
            icon = AssetPreview.GetMiniThumbnail(comp);
            componentIconCache[type] = icon;
        }
        return icon;
    }
}
