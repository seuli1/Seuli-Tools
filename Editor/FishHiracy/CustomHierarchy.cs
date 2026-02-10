using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Seulitools
{
    [InitializeOnLoad]
    public static class CustomHierarchy
    {
        private static readonly float iconSize = 14f;
        private static readonly float iconSpacing = 2f;
        private static readonly Color prefabTint = new Color(0.2f, 0.5f, 1f, 0.08f);
        private static readonly Color disabledTint = new Color(1f, 1f, 1f, 0.35f);
        private static readonly Color missingTint = new Color(1f, 1f, 1f, 0.65f);
        private static readonly Color childCountBg = new Color(0f, 0f, 0f, 0.2f);
        private static GUIStyle childCountStyle;
        private static GUIStyle missingIconStyle;
        private static Texture2D missingIcon;
        private static Texture2D visibleIcon;
        private static Texture2D hiddenIcon;
        private static readonly GUIContent tempContent = new GUIContent();
        private static readonly Dictionary<System.Type, Texture2D> componentIconCache = new Dictionary<System.Type, Texture2D>();
        private static readonly Dictionary<int, Component[]> componentCache = new Dictionary<int, Component[]>();

        static CustomHierarchy()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.hierarchyChanged += ClearComponentCache;
            Undo.undoRedoPerformed += ClearComponentCache;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#if UNITY_2021_2_OR_NEWER
            ObjectChangeEvents.changesPublished += OnObjectChange;
#endif
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            EnsureStylesAndIcons();

            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;

            bool showActiveToggle = CustomHierarchySettings.ShowActiveToggle;
            bool showIndicators = CustomHierarchySettings.ShowIndicators;
            bool showComponentIcons = CustomHierarchySettings.ShowComponentIcons;
            bool showChildCount = CustomHierarchySettings.ShowChildCount;

            float xOffset = selectionRect.xMax - 2f;
            float yOffset = selectionRect.y + (selectionRect.height - iconSize) * 0.5f;

            if (showActiveToggle)
            {
                xOffset -= iconSize;
                Rect eyeRect = new Rect(xOffset, yOffset, iconSize, iconSize);
                Texture2D eyeIcon = obj.activeSelf ? visibleIcon : hiddenIcon;

                if (GUI.Button(eyeRect, eyeIcon, GUIStyle.none))
                {
                    Undo.RecordObject(obj, "Toggle Active");
                    obj.SetActive(!obj.activeSelf);
                    EditorUtility.SetDirty(obj);
                }
            }


            if (PrefabUtility.IsAnyPrefabInstanceRoot(obj))
            {
                EditorGUI.DrawRect(selectionRect, prefabTint);
            }

            if (showIndicators || showComponentIcons)
            {
                Component[] components = GetComponentsCached(obj, instanceID);

                for (int i = components.Length - 1; i >= 0; i--)
                {
                    Component comp = components[i];

                    if (comp == null && showIndicators)
                    {
                        if (missingIcon != null)
                        {
                            xOffset -= iconSize + iconSpacing;
                            Rect rect = new Rect(xOffset, yOffset, iconSize, iconSize);
                            Color prevColor = GUI.color;
                            GUI.color = missingTint;
                            GUI.Label(rect, missingIcon, missingIconStyle);
                            GUI.color = prevColor;
                        }
                        continue;
                    }

                    if (comp != null && !(comp is Transform) && showComponentIcons)
                    {
                        xOffset -= iconSize + iconSpacing;
                        Rect rect = new Rect(xOffset, yOffset, iconSize, iconSize);

                        Texture2D icon = GetComponentIcon(comp);
                        string tooltip = comp.GetType().Name;

                        if (comp is SkinnedMeshRenderer smr)
                        {
                            tooltip += $"\nVerts: {smr.sharedMesh?.vertexCount ?? 0}, Mats: {smr.sharedMaterials.Length}";
                        }

                        tempContent.image = icon;
                        tempContent.tooltip = tooltip;

                        bool isToggleable = comp is Behaviour || comp is Collider;
                        if (isToggleable)
                        {
                            bool enabledState = comp is Behaviour b ? b.enabled : (comp as Collider).enabled;
                            GUI.color = enabledState ? Color.white : disabledTint;
                            bool newState = GUI.Toggle(rect, enabledState, tempContent, GUIStyle.none);
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
                            GUI.Label(rect, tempContent);
                        }
                    }
                }
            }


            if (showChildCount && obj.transform.childCount > 0)
            {
                float width = 22f;
                float height = 14f;
                xOffset -= width + iconSpacing;
                Rect rect = new Rect(xOffset, selectionRect.y + (selectionRect.height - height) * 0.5f, width, height);
                EditorGUI.DrawRect(rect, childCountBg);
                GUI.Label(rect, obj.transform.childCount.ToString(), childCountStyle);
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

        private static Component[] GetComponentsCached(GameObject obj, int instanceID)
        {
            if (!componentCache.TryGetValue(instanceID, out Component[] components) || components == null)
            {
                components = obj.GetComponents<Component>();
                componentCache[instanceID] = components;
            }
            return components;
        }

        private static void ClearComponentCache()
        {
            componentCache.Clear();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            ClearComponentCache();
        }

#if UNITY_2021_2_OR_NEWER
        private static void OnObjectChange(ref ObjectChangeEventStream stream)
        {
            ClearComponentCache();
        }
#endif

        private static void EnsureStylesAndIcons()
        {
            if (childCountStyle == null)
            {
                childCountStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 9
                };
                childCountStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            }


            if (missingIconStyle == null)
            {
                missingIconStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            }

            if (missingIcon == null)
            {
                missingIcon = EditorGUIUtility.IconContent("console.erroricon.sml").image as Texture2D;
            }

            if (visibleIcon == null)
            {
                visibleIcon = EditorGUIUtility.IconContent("d_scenevis_visible_hover").image as Texture2D;
            }

            if (hiddenIcon == null)
            {
                hiddenIcon = EditorGUIUtility.IconContent("d_scenevis_hidden_hover").image as Texture2D;
            }

        }
    }
}
