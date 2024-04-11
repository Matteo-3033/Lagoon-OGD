using System.Reflection;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Editor
{
    public static class Build
    {
        private static readonly string[] ServerScenes =
        {
            Scenes.Master
        };
        
        private static readonly string[] MatchScenes =
        {
            Scenes.Menu, Scenes.Lobby, Scenes.Round
        };
        
        [MenuItem("Build/Build All")]
        public static void BuildAll()
        {
            BuildLinuxMasterServer();
            BuildLinuxServer();
            BuildWindows();
        }
        
        [MenuItem("Build/Windows/Build All")]
        public static void BuildWindows()
        {
            BuildWindowsMasterServer();
            BuildWindowsServer();
            BuildWindowsClient();
        }
        
        [MenuItem("Build/Windows/Master Server")]
        public static void BuildWindowsMasterServer()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = ServerScenes,
                locationPathName = "Builds/Windows/Master/Master.exe",
                target = BuildTarget.StandaloneWindows64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log("Building Master Server (Windows)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Built Server (Windows).");
        }
        
        [MenuItem("Build/Windows/Server")]
        public static void BuildWindowsServer()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = MatchScenes,
                locationPathName = "Builds/Windows/Server/Server.exe",
                target = BuildTarget.StandaloneWindows64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log("Building Server (Windows)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Built Server (Windows).");
        }

        [MenuItem("Build/Linux/Master Server")]
        public static void BuildLinuxMasterServer()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = ServerScenes,
                locationPathName = "Builds/Linux/Master/Master.x86_64",
                target = BuildTarget.StandaloneLinux64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log("Building Master Server (Linux)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Built Server (Linux).");
        }
        
        [MenuItem("Build/Linux/Server")]
        public static void BuildLinuxServer()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = MatchScenes,
                locationPathName = "Builds/Linux/Server/Server.x86_64",
                target = BuildTarget.StandaloneLinux64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log("Building Server (Linux)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Built Server (Linux).");
        }


        [MenuItem("Build/Windows/Client")]
        public static void BuildWindowsClient()
        {

            var method = typeof(BuildPlayerWindow.DefaultBuildMethods).GetMethod("GetBuildPlayerOptionsInternal", BindingFlags.NonPublic | BindingFlags.Static);

            var buildPlayerOptions = (BuildPlayerOptions) method!.Invoke(
                null, 
                new object[] { false, new BuildPlayerOptions() }
            );
            buildPlayerOptions.scenes = MatchScenes;
            buildPlayerOptions.locationPathName = "Builds/Windows/Client/Client.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Player;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;
            
            Debug.Log("Building Client (Windows)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Built Client (Windows).");
        }
    }
}