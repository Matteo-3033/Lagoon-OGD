using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Build
    {
        private static readonly string[] Scenes =
        {
            Utils.Scenes.Connection, Utils.Scenes.Menu
        };
        
        [MenuItem("Build/Build All")]
        public static void BuildAll()
        {
            BuildWindowsServer();
            BuildLinuxServer();
            BuildWindowsClient();
        }
        
        [MenuItem("Build/Build Windows")]
        public static void BuildWindows()
        {
            BuildWindowsServer();
            BuildWindowsClient();
        }
        
        [MenuItem("Build/Build Server (Windows)")]
        public static void BuildWindowsServer()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = "Builds/Windows/Server/Server.exe",
                target = BuildTarget.StandaloneWindows64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log("Building Server (Windows)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Built Server (Windows).");
        }

        [MenuItem("Build/Build Server (Linux)")]
        public static void BuildLinuxServer()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = "Builds/Linux/Server/Server.x86_64",
                target = BuildTarget.StandaloneLinux64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log("Building Server (Linux)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Built Server (Linux).");
        }


        [MenuItem("Build/Build Client (Windows)")]
        public static void BuildWindowsClient()
        {

            var method = typeof(BuildPlayerWindow.DefaultBuildMethods).GetMethod("GetBuildPlayerOptionsInternal", BindingFlags.NonPublic | BindingFlags.Static);

            var buildPlayerOptions = (BuildPlayerOptions) method!.Invoke(
                null, 
                new object[] { false, new BuildPlayerOptions() }
            );
            buildPlayerOptions.scenes = Scenes;
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