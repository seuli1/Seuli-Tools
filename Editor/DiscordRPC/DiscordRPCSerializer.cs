#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using VRC.Core;

namespace Seulitools
{
    [InitializeOnLoad]
    public class DiscordRPCSerializer
    {
        private static readonly DiscordRPCAPI.RichPresence presence = new DiscordRPCAPI.RichPresence();

        private static TimeSpan time = (DateTime.UtcNow - new DateTime(1970, 1, 1));
        private static long timestamp = (long)time.TotalSeconds;
        private static string GameName = Application.productName;

        private static RpcState rpcState = RpcState.EDITMODE;

        static DiscordRPCSerializer()
        {
            if(!EditorPrefs.HasKey("discordRPC"))
            {
                EditorPrefs.SetBool("discordRPC", true);
            }

            if (EditorPrefs.GetBool("discordRPC"))
            {
                Log("Starting discord rpc");
                DiscordRPCAPI.EventHandlers eventHandlers = default(DiscordRPCAPI.EventHandlers);
                DiscordRPCAPI.Initialize("1380322851500068886", ref eventHandlers, false, string.Empty);
                updateDRPC();
            }
        }

        public static void updateDRPC()
        {
            Log("Updating everything");
            presence.details = string.Format("In Project: {0}", GameName);
            presence.state = "Currently " + rpcState.StateName();
            presence.startTimestamp = timestamp;
            presence.largeImageKey = "fish";
            presence.largeImageText = "Made by Seuli";
            DiscordRPCAPI.UpdatePresence(presence);
            
        }

        public static void updateState(RpcState state)
        {
            Log("Updating state to '" + state.StateName() + "'");
            rpcState = state;
            presence.state = "State: " + state.StateName();
            DiscordRPCAPI.UpdatePresence(presence);
        }

        public static void ResetTime()
        {
            Log("Reseting timer");
            time = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            timestamp = (long)time.TotalSeconds;
            presence.startTimestamp = timestamp;

            DiscordRPCAPI.UpdatePresence(presence);
        }

        private static void Log(string message)
        {
            Debug.Log("[Seulitools] DiscordRPC: " + message);
        }
    }
}
#endif