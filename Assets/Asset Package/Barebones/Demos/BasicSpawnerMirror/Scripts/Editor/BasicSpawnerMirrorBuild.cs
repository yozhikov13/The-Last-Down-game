using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Barebones.MasterServer.Examples.BasicSpawnerMirror
{
    public class BasicSpawnerMirrorBuild
    {
        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner Mirror/All")]
        private static void BuildBoth()
        {
            BuildMasterForWindows();
            BuildMasterAndSpawnerForWindows();
            BuildSpawnerForWindows();
            BuildRoomForWindows();
            BuildClientForWindows();
        }

        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner Mirror/Master Server and Spawner")]
        private static void BuildMasterAndSpawnerForWindows()
        {
            string buildFolder = Path.Combine("Builds", "BasicSpawnerMirror", "MasterAndSpawner");
            string roomExePath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "BasicSpawnerMirror", "Room", "Room.exe");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Barebones/Demos/BasicSpawnerMirror/Scenes/MasterAndSpawner/MasterAndSpawner.unity" },
                locationPathName = Path.Combine(buildFolder, "MasterAndSpawner.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("@echo off\n");
                arguments.Append("start \"Basic Spawner Mirror - Master and Spawner\" ");
                arguments.Append("MasterAndSpawner.exe ");
                arguments.Append($"{Msf.Args.Names.StartMaster} ");
                arguments.Append($"{Msf.Args.Names.StartSpawner} ");
                arguments.Append($"{Msf.Args.Names.StartClientConnection} ");
                arguments.Append($"{Msf.Args.Names.MasterIp} {Msf.Args.MasterIp} ");
                arguments.Append($"{Msf.Args.Names.MasterPort} {Msf.Args.MasterPort} ");
                arguments.Append($"{Msf.Args.Names.DontSpawnInBatchmode} ");
                arguments.Append($"{Msf.Args.Names.RoomExecutablePath} {roomExePath} ");

                File.WriteAllText(Path.Combine(buildFolder, "Start Master Server and Spawner.bat"), arguments.ToString());

                Debug.Log("Master Server build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Master Server build failed");
            }
        }

        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner Mirror/Master Server")]
        private static void BuildMasterForWindows()
        {
            string buildFolder = Path.Combine("Builds", "BasicSpawnerMirror", "MasterServer");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Barebones/Demos/BasicSpawnerMirror/Scenes/MasterServer/MasterServer.unity" },
                locationPathName = Path.Combine(buildFolder, "MasterServer.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("@echo off\n");
                arguments.Append("start \"Basic Spawner Mirror - Master Server\" ");
                arguments.Append("MasterServer.exe ");
                arguments.Append($"{Msf.Args.Names.StartMaster} ");
                arguments.Append($"{Msf.Args.Names.MasterIp} {Msf.Args.MasterIp} ");

                File.WriteAllText(Path.Combine(buildFolder, "Start Master Server.bat"), arguments.ToString());

                Debug.Log("Master Server build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Master Server build failed");
            }
        }

        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner Mirror/Spawner")]
        private static void BuildSpawnerForWindows()
        {
            string buildFolder = Path.Combine("Builds", "BasicSpawnerMirror", "Spawner");
            string roomExePath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "BasicSpawnerMirror", "Room", "Room.exe");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Barebones/Demos/BasicSpawnerMirror/Scenes/Spawner/Spawner.unity" },
                locationPathName = Path.Combine(buildFolder, "Spawner.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("@echo off\n");
                arguments.Append("start \"Basic Spawner Mirror - Spawner\" ");
                arguments.Append("Spawner.exe ");
                arguments.Append($"{Msf.Args.Names.StartSpawner} ");
                arguments.Append($"{Msf.Args.Names.StartClientConnection} ");
                arguments.Append($"{Msf.Args.Names.MasterIp} {Msf.Args.MasterIp} ");
                arguments.Append($"{Msf.Args.Names.MasterPort} {Msf.Args.MasterPort} ");
                arguments.Append($"{Msf.Args.Names.DontSpawnInBatchmode} ");
                arguments.Append($"{Msf.Args.Names.RoomExecutablePath} {roomExePath} ");

                File.WriteAllText(Path.Combine(buildFolder, "Start Spawner.bat"), arguments.ToString());

                Debug.Log("Spawner build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Spawner build failed");
            }
        }

        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner Mirror/Room")]
        private static void BuildRoomForWindows()
        {
            string buildFolder = Path.Combine("Builds", "BasicSpawnerMirror", "Room");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] {
                    "Assets/Barebones/Demos/BasicSpawnerMirror/Scenes/Room/RoomStart.unity",
                    "Assets/Barebones/Demos/BasicSpawnerMirror/Scenes/Room/RoomOnline.unity"
                },
                locationPathName = Path.Combine(buildFolder, "Room.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.ShowBuiltPlayer/* | BuildOptions.Development*/
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("@echo off\n");
                arguments.Append("start \"Basic Spawner Mirror - Room\" ");
                arguments.Append("Room.exe ");
                arguments.Append($"{Msf.Args.Names.StartClientConnection} ");
                arguments.Append($"{Msf.Args.Names.MasterIp} 127.0.0.1 ");
                arguments.Append($"{Msf.Args.Names.MasterPort} 5000 ");
                arguments.Append($"{Msf.Args.Names.RoomIp} 127.0.0.1 ");
                arguments.Append($"{Msf.Args.Names.RoomPort} 7777 ");

                File.WriteAllText(Path.Combine(buildFolder, "Start Room.bat"), arguments.ToString());

                Debug.Log("Room build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Room build failed");
            }
        }

        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner Mirror/Client")]
        private static void BuildClientForWindows()
        {
            string buildFolder = Path.Combine("Builds", "BasicSpawnerMirror", "Client");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] {
                    "Assets/Barebones/Demos/BasicSpawnerMirror/Scenes/Client/Client.unity",
                    "Assets/Barebones/Demos/BasicSpawnerMirror/Scenes/Room/RoomStart.unity",
                    "Assets/Barebones/Demos/BasicSpawnerMirror/Scenes/Room/RoomOnline.unity"
                },
                locationPathName = Path.Combine(buildFolder, "Client.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Client build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Client build failed");
            }
        }
    }
}