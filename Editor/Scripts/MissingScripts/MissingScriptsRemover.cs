#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Seulitools
{
    public class MissingScriptsRemover : Editor
    {
        [MenuItem("SeuliTools/Tools/Remove Missing Scripts", false, 110)]
        public static void RemoveMissingScripts()
        {
            bool confirm = EditorUtility.DisplayDialog("Remove Missing Scripts", "This will remove missing scripts from all GameObjects in the scene. Continue?", "Remove", "Cancel");
            if (!confirm)
            {
                return;
            }

            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>(true);

            int totalComponentCount = 0;

            foreach (GameObject go in gameObjects)
            {
                int componentCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);

                if (componentCount > 0)
                {
                    Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");

                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

                    totalComponentCount += componentCount;
                }
            }

            string message = string.Format("Removed {0} missing scripts from {1} game object(s) in the hierarchy.", totalComponentCount, gameObjects.Length);
            EditorUtility.DisplayDialog("Remove Missing Scripts", message, "OK");
        }

        [MenuItem("SeuliTools/Tools/Remove Missing Scripts", true, 110)]
        private static bool ValidateRemoveMissingScripts()
        {
            return true;
        }
    }

}
#endif