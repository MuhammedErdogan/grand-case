using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace _GrandGames.Util
{
    public static class LocalFileHelper
    {
        private static string ProjectRoot =>
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        private static string LevelsRoot =>
            Path.Combine(ProjectRoot, "Levels");

        private static string Normalize(string rel)
        {
            rel = (rel ?? string.Empty).Replace("\\", "/").TrimStart('/');
            if (rel.Length == 0 || rel.Contains(".."))
            {
                throw new Exception($"Unsafe relative path: {rel}");
            }

            Debug.Log($"Normalize: {rel}");
            return rel;
        }

        private static string ToUri(string absolutePathLike)
        {
            var path = absolutePathLike.Replace("\\", "/");
            if (path.StartsWith("jar:") || path.StartsWith("file:"))
            {
                Debug.Log($"ToUri: {path}");
                return path;
            }

            Debug.Log($"ToUri: {path} -> {new Uri(path).AbsoluteUri}");

            return new Uri(path).AbsoluteUri;
        }

        private static string EncodePathPreserveSlashes(string rel)
        {
            return string.Join("/",
                rel.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(Uri.EscapeDataString));
        }

        private static string StreamingAssetUrl(string relativePath)
        {
            var rel = Normalize(relativePath);
            var encRel = EncodePathPreserveSlashes(rel); // <= kritik

            // Editor Assets/Levels ten okur, Build StreamingAssets ten okur.
#if UNITY_EDITOR
            var basePath = LevelsRoot.Replace("\\", "/");
            return ToUri($"{basePath}/{encRel}");

#else
            var sa = Application.streamingAssetsPath.Replace("\\", "/");
            // jar:file:///...!/assets/levels_1_500/level_1_updated   (uzantı şart değil)
            Debug.Log($"StreamingAssetsPath: {sa}/{encRel}");
            return $"{sa}/{encRel}";
#endif
        }

        private static string PersistentAbsolute(string path)
        {
            var abs = Path.IsPathRooted(path) ?
                path :
                Path.Combine(Application.persistentDataPath, path);

            return abs;
        }

        public static async UniTask<string> GetResourceText(string resourcePathWithoutExtension, bool unloadAfterRead = true, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var req = Resources.LoadAsync<TextAsset>(resourcePathWithoutExtension);
            await req.ToUniTask(cancellationToken: ct);

            if (req.asset is not TextAsset ta)
            {
                throw new Exception($"TextAsset not found: {resourcePathWithoutExtension}");
            }

            var text = ta.text;
            if (unloadAfterRead)
            {
                Resources.UnloadAsset(ta);
            }

            return text;
        }

        public static async UniTask<string> ReadPersistentText(string relativeOrAbsolutePath, CancellationToken ct = default)
        {
            var abs = PersistentAbsolute(relativeOrAbsolutePath);
            return await File.ReadAllTextAsync(abs, ct);
        }

        // STREAMING → PERSISTENT (atomic), büyük dosyalarda RAM'e almadan:
        public static async UniTask CopyStreamingToPersistentAtomic(
            string streamingRelative,
            string persistentRelative,
            IProgress<float> progress = null,
            CancellationToken ct = default)
        {
            var url = StreamingAssetUrl(streamingRelative);
            var dst = PersistentAbsolute(persistentRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);

            var tmp = dst + ".tmp";
            using var req = UnityWebRequest.Get(url);
            req.downloadHandler = new DownloadHandlerFile(tmp, true);
            await req.SendWebRequest().ToUniTask(progress: progress, cancellationToken: ct);

            if (req.result != UnityWebRequest.Result.Success)
            {
                if (File.Exists(tmp))
                {
                    File.Delete(tmp);
                }

                throw new Exception(req.error);
            }

            if (File.Exists(dst))
            {
                File.Delete(dst);
            }

            File.Move(tmp, dst);
        }
    }
}