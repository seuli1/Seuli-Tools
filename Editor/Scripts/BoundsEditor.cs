#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

using VRC.SDK3.Avatars.Components;
using System.Collections.Generic;



namespace Seulitools
{
    public class BoundsEditor : EditorWindow
    {
        GameObject parent;
        VRCAvatarDescriptor[] avatarDescriptorsFromScene;
        SkinnedMeshRenderer[] cachedRenderers;
        int cachedParentInstanceId = 0;
        bool renderersDirty = true;

        Vector3 newBoundsCenter;
        Vector3 newBoundsExtent = new Vector3(1, 1, 1);

        Vector2 scrollPos;
        Vector2 scrollPosition;

        bool centerLink = false;
        bool extentLink = true;
        private int lastChangedAxis = -1;

        List<bool> individualLinks = new List<bool>();



        [MenuItem("SeuliTools/Tools/Bounds Editor", false, 100)]


        public static void ShowWindow()
        {
            EditorWindow w = EditorWindow.GetWindow(typeof(BoundsEditor), false, "Bounds Editor");
            w.titleContent = new GUIContent("Bounds Editor");

        }

        private void OnEnable()
        {
            RefreshDescriptors();
            RefreshRenderers();
            EditorApplication.hierarchyChanged += MarkRenderersDirty;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= MarkRenderersDirty;
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawTitle("Bounds Editor", "Edit SkinnedMeshRenderer bounds for VRChat avatars.");

            DrawAvatarSelector();

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Per Renderer", EditorStyles.boldLabel);

            if (parent != null)
            {
                EnsureRenderersCached();
                SkinnedMeshRenderer[] renderers = cachedRenderers ?? System.Array.Empty<SkinnedMeshRenderer>();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Renderers", renderers.Length.ToString(), EditorStyles.miniLabel);
                if (parent != null)
                {
                    EditorGUILayout.LabelField("Target", parent.name, EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();

                if (renderers.Length == 0)
                {
                    EditorGUILayout.HelpBox("No SkinnedMeshRenderers found on the target.", MessageType.Info);
                }
                else
                {

                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(false));

                    EnsureIndividualLinks(renderers.Length);

                    for (int i = 0; i < renderers.Length; i++)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.ObjectField(renderers[i], typeof(SkinnedMeshRenderer), true);

                        Mesh mesh = renderers[i].sharedMesh;
                        if (mesh != null)
                        {
                            EditorGUILayout.LabelField("Mesh", mesh.name + " | V: " + mesh.vertexCount, EditorStyles.miniLabel);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Mesh", "None", EditorStyles.miniLabel);
                        }

                        Vector3 center = renderers[i].localBounds.center;
                        Vector3 extent = renderers[i].localBounds.extents;

                        EditorGUI.BeginChangeCheck();
                        Vector3WithLinkForLoop("Center", ref center, i * 2);
                        Vector3WithLinkForLoop("Extent", ref extent, i * 2 + 1);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(renderers[i], "Edit Bounds");
                            renderers[i].localBounds = new Bounds { center = center, extents = extent };
                            EditorUtility.SetDirty(renderers[i]);
                        }

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.Space(16);
                    EditorGUILayout.LabelField("Batch Update", EditorStyles.boldLabel);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    GUILayout.Label("Set all to:", EditorStyles.boldLabel);

                    if (GUILayout.Button("Use First Renderer"))
                    {
                        Bounds firstBounds = renderers[0].localBounds;
                        newBoundsCenter = firstBounds.center;
                        newBoundsExtent = firstBounds.extents;
                    }

                    Vector3WithLink("Center", ref newBoundsCenter, ref centerLink);
                    Vector3WithLink("Extent", ref newBoundsExtent, ref extentLink);

                    if (GUILayout.Button("Set"))
                    {
                        Undo.RecordObjects(renderers, "Set Bounds");
                        foreach (var item in renderers)
                        {

                            item.localBounds = new Bounds { center = newBoundsCenter, extents = newBoundsExtent };

                            EditorUtility.SetDirty(item);
                        }

                    }

                    EditorGUILayout.EndVertical();
                }

            }
            else
            {
                EditorGUILayout.HelpBox("Select an avatar or a GameObject with SkinnedMeshRenderers.", MessageType.Info);
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        void Vector3WithLink(string name, ref Vector3 vector, ref bool useLink)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(name, GUILayout.Width(50f));

            bool newLink = GUILayout.Toggle(useLink, useLink ? "Linked" : "Unlinked", EditorStyles.miniButton, GUILayout.Width(68f));
            useLink = newLink;

            EditorGUI.BeginChangeCheck();

            Vector3 oldValue = vector;
            vector = EditorGUILayout.Vector3Field("", vector);

            if (EditorGUI.EndChangeCheck())
            {
                ApplyLinkedVector3(ref vector, oldValue, useLink);
            }

            EditorGUILayout.EndHorizontal();
        }


        void Vector3WithLinkForLoop(string name, ref Vector3 vector, int i)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(name, GUILayout.Width(50f));

            bool newLink = GUILayout.Toggle(individualLinks[i], individualLinks[i] ? "Linked" : "Unlinked", EditorStyles.miniButton, GUILayout.Width(68f));
            individualLinks[i] = newLink;

            EditorGUI.BeginChangeCheck();

            Vector3 oldValue = vector;
            vector = EditorGUILayout.Vector3Field("", vector);

            if (EditorGUI.EndChangeCheck())
            {
                ApplyLinkedVector3(ref vector, oldValue, individualLinks[i]);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void RefreshDescriptors()
        {
            avatarDescriptorsFromScene = Object.FindObjectsOfType<VRCAvatarDescriptor>(true);
            if (parent == null && avatarDescriptorsFromScene.Length > 0)
            {
                parent = avatarDescriptorsFromScene[0].gameObject;
            }
        }

        private void DrawTitle(string title, string subtitle)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(title, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(subtitle, EditorStyles.miniLabel);
            EditorGUILayout.Space(4f);
        }

        private void DrawAvatarSelector()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            parent = (GameObject)EditorGUILayout.ObjectField("Avatar", parent, typeof(GameObject), true);
            if (GUILayout.Button("Use Selection", GUILayout.Width(110f)))
            {
                if (Selection.activeGameObject != null)
                {
                    parent = Selection.activeGameObject;
                    RefreshRenderers();
                }
            }
            if (GUILayout.Button("Refresh", GUILayout.Width(70f)))
            {
                RefreshDescriptors();
                RefreshRenderers();
            }
            EditorGUILayout.EndHorizontal();

            if (avatarDescriptorsFromScene == null || avatarDescriptorsFromScene.Length == 0)
            {
                return;
            }

            int currentIndex = -1;
            List<string> names = new List<string>(avatarDescriptorsFromScene.Length);
            List<VRCAvatarDescriptor> validDescriptors = new List<VRCAvatarDescriptor>(avatarDescriptorsFromScene.Length);

            for (int i = 0; i < avatarDescriptorsFromScene.Length; i++)
            {
                if (avatarDescriptorsFromScene[i] == null)
                {
                    continue;
                }

                validDescriptors.Add(avatarDescriptorsFromScene[i]);
                names.Add(avatarDescriptorsFromScene[i].name);

                if (avatarDescriptorsFromScene[i].gameObject == parent)
                {
                    currentIndex = validDescriptors.Count - 1;
                }
            }

            if (validDescriptors.Count > 0)
            {
                int newIndex = EditorGUILayout.Popup("Scene Avatars", Mathf.Max(currentIndex, 0), names.ToArray());
                if (newIndex != currentIndex && newIndex >= 0)
                {
                    parent = validDescriptors[newIndex].gameObject;
                    RefreshRenderers();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void MarkRenderersDirty()
        {
            renderersDirty = true;
        }

        private void EnsureRenderersCached()
        {
            int currentId = parent != null ? parent.GetInstanceID() : 0;
            if (!renderersDirty && cachedRenderers != null && cachedParentInstanceId == currentId)
            {
                return;
            }

            RefreshRenderers();
        }

        private void RefreshRenderers()
        {
            if (parent == null)
            {
                cachedRenderers = null;
                cachedParentInstanceId = 0;
                renderersDirty = false;
                return;
            }

            cachedRenderers = parent.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            cachedParentInstanceId = parent.GetInstanceID();
            renderersDirty = false;
        }

        private void ApplyLinkedVector3(ref Vector3 vector, Vector3 oldValue, bool isLinked)
        {
            if (!isLinked)
            {
                return;
            }

            if (vector.x != oldValue.x) lastChangedAxis = 0;
            else if (vector.y != oldValue.y) lastChangedAxis = 1;
            else if (vector.z != oldValue.z) lastChangedAxis = 2;

            if (lastChangedAxis != -1)
            {
                float newValue = vector[lastChangedAxis];
                vector = new Vector3(newValue, newValue, newValue);
            }
        }

        private void EnsureIndividualLinks(int rendererCount)
        {
            int expectedCount = rendererCount * 2;
            if (individualLinks.Count == expectedCount)
            {
                return;
            }

            individualLinks.Clear();
            for (int i = 0; i < rendererCount; i++)
            {
                individualLinks.Add(false);
                individualLinks.Add(true);
            }
        }

    }

}

    #endif