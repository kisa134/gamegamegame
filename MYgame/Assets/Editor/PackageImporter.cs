using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Tools
{
    /// <summary>
    /// Headless .unitypackage import.
    ///
    /// The `-importPackage` command line switch only *schedules* the import; `-quit` then
    /// tears the editor down before it finishes, leaving nothing on disk. Driving it from
    /// a method instead lets us import, refresh, and exit in the right order.
    ///
    /// Usage: -executeMethod Game.Tools.PackageImporter.ImportFromArgs -importPaths "a;b"
    /// (no -quit; this exits by itself)
    /// </summary>
    public static class PackageImporter
    {
        public static void ImportFromArgs()
        {
            var paths = ReadPathsArgument();
            if (paths.Length == 0)
            {
                Debug.LogError("[PackageImporter] No -importPaths argument supplied.");
                EditorApplication.Exit(2);
                return;
            }

            var failed = false;
            AssetDatabase.importPackageFailed += (name, message) =>
            {
                Debug.LogError($"[PackageImporter] FAILED {name}: {message}");
                failed = true;
            };
            AssetDatabase.importPackageCompleted += name =>
                Debug.Log($"[PackageImporter] COMPLETED {name}");

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    Debug.LogError($"[PackageImporter] Missing package: {path}");
                    failed = true;
                    continue;
                }

                Debug.Log($"[PackageImporter] Importing {Path.GetFileName(path)} ...");
                try
                {
                    // AssetDatabase.ImportPackage is asynchronous: it queues the work and
                    // returns, so a batchmode process that exits promptly imports nothing.
                    // ImportPackageImmediately is the synchronous sibling; it is internal,
                    // hence the reflection, with the async call as a fallback.
                    var immediate = typeof(AssetDatabase).GetMethod(
                        "ImportPackageImmediately",
                        System.Reflection.BindingFlags.Static
                        | System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Public);

                    if (immediate != null)
                    {
                        immediate.Invoke(null, new object[] { path });
                        Debug.Log("[PackageImporter]   (synchronous path)");
                    }
                    else
                    {
                        AssetDatabase.ImportPackage(path, false);
                        Debug.LogWarning("[PackageImporter]   (async fallback - may not finish)");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PackageImporter] Threw on {path}: {ex}");
                    failed = true;
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();

            Debug.Log(failed
                ? "[PackageImporter] Finished WITH ERRORS."
                : "[PackageImporter] Finished cleanly.");

            EditorApplication.Exit(failed ? 1 : 0);
        }

        private static string[] ReadPathsArgument()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-importPaths")
                {
                    return args[i + 1].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return Array.Empty<string>();
        }
    }
}
