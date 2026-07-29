using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Unity.Editor
{
    /// <summary>Headless Windows player build. Entry point for -executeMethod.</summary>
    public static class BuildScript
    {
        private const string ScenePath = "Assets/Scenes/Prototype.unity";

        public static void BuildWindows()
        {
            var output = Path.Combine(
                Path.GetDirectoryName(UnityEngine.Application.dataPath) ?? ".",
                "Builds", "Windows", "MYgame.exe");

            Directory.CreateDirectory(Path.GetDirectoryName(output));

            // Mono starts in seconds; IL2CPP costs ten minutes and buys nothing for a playtest.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.runInBackground = true;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            Debug.Log($"[Build] {summary.result} — {summary.totalSize / (1024 * 1024)} MB, " +
                      $"{summary.totalErrors} errors, output: {output}");

            // BuildPlayer does not set the process exit code. Without this an failed build
            // reports green and silently produces no exe.
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
