#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GrandGames.Editor
{
    public sealed class LevelAssetPostProcessor : AssetPostprocessor, IPreprocessBuildWithReport
    {
        private const string SRC_OUTSIDE_REL = "Levels/levels_1_500"; // Project root'a göre
        private const string DEST_STREAMING = "Assets/StreamingAssets/levels_1_500";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            FullSync(showSummary: true);
        }

        [MenuItem("Tools/Levels/Sync Remote → StreamingAssets (Outside only)")] //test
        public static void FullSyncMenu() => FullSync(showSummary: true);

        private static void FullSync(bool showSummary)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var srcOutsideAbs = Path.GetFullPath(Path.Combine(projectRoot, SRC_OUTSIDE_REL)).Replace('\\', '/');

            if (!Directory.Exists(srcOutsideAbs))
            {
                Debug.LogWarning($"[LevelSync] Outside source not found: {srcOutsideAbs}\n" +
                                 $"Create '{SRC_OUTSIDE_REL}' under project root and drop level_###_updated files.");

                return; // StreamingAssets klasörünü boşuna yaratma
            }

            var files = Directory.GetFiles(srcOutsideAbs, "*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (files.Count == 0)
            {
                if (showSummary)
                {
                    Debug.Log("[LevelSync] No files to sync.");
                }

                return;
            }

            var dest = NormalizePath(DEST_STREAMING);
            Directory.CreateDirectory(dest); // Artık kopya var, oluştur

            int copied = 0, skipped = 0;
            foreach (var srcFile in files)
            {
                var name = Path.GetFileName(srcFile);
                var dstPath = Path.Combine(dest, name);

                if (CopyIfDifferent(srcFile, dstPath))
                {
                    copied++;
                }
                else
                {
                    skipped++;
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            if (showSummary)
            {
                Debug.Log($"[LevelSync] Done. Copied: {copied}, Skipped: {skipped}\n→ {dest}");
            }
        }

        private static bool CopyIfDifferent(string src, string dst)
        {
            try
            {
                var dstDir = Path.GetDirectoryName(dst);
                if (!string.IsNullOrEmpty(dstDir) && !Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                }

                if (File.Exists(dst))
                {
                    var si = new FileInfo(src);
                    var di = new FileInfo(dst);
                    if (si.Length == di.Length && si.LastWriteTimeUtc <= di.LastWriteTimeUtc)
                    {
                        return false;
                    }
                }

                File.Copy(src, dst, overwrite: true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LevelSync] Copy failed: {src} → {dst}\n{e}");
                return false;
            }
        }

        private static string NormalizePath(string p) =>
            string.IsNullOrEmpty(p) ?
                string.Empty :
                p.Replace('\\', '/').TrimEnd('/');
    }
}
#endif