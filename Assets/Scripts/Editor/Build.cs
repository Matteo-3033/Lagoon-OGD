using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Build
    {
        private static readonly string[] Scenes =
        {
            Utils.Scenes.Connection, Utils.Scenes.MainMenu
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
                options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode
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
                options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode
            };

            Debug.Log("Building Server (Linux)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Built Server (Linux).");
        }


        [MenuItem("Build/Build Client (Windows)")]
        public static void BuildWindowsClient()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = "Builds/Windows/Client/Client.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log("Building Client (Windows)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Built Client (Windows).");
        }
    }
}