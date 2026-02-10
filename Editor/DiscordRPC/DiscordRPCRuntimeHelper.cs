#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Seulitools
{
    [InitializeOnLoadAttribute]
    public static class DiscordRpcRuntimeHelper
    {

        static DiscordRpcRuntimeHelper()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                DiscordRPCSerializer.updateState(RpcState.EDITMODE);
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
            {
                DiscordRPCSerializer.updateState(RpcState.PLAYMODE);
            }
        }
    }

    internal class UploadPreHook : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -1000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            DiscordRPCSerializer.updateState(RpcState.UPLOADAVATAR);
            return true;
        }
    }

    internal class UploadPostHook : IVRCSDKPostprocessAvatarCallback
    {
        public int callbackOrder => -1000;

        public void OnPostprocessAvatar()
        {
            DiscordRPCSerializer.updateState(RpcState.EDITMODE);
        }
    }
}
#endif