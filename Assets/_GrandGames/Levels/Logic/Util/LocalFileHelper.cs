namespace _GrandGames.Levels.Logic.Util
{
    using System;
    using System.IO;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    public static class LocalFileHelper
    {
        private static string LevelsRoot => Path.Combine(Application.dataPath, "Levels");

        private static string Normalize(string rel)
        {
            rel = (rel ?? string.Empty).Replace("\\", "/").TrimStart('/');
            if (rel.Length == 0 || rel.Contains(".."))
            {
                throw new Exception($"Unsafe relative path: {rel}");
            }

            return rel;
        }

        private static string WithJsonExt(string rel)
        {
            return rel.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ?
                rel :
                rel + ".json";
        }

        private static string ToUri(string absolutePathLike)
        {
            var path = absolutePathLike.Replace("\\", "/");
            if (path.StartsWith("jar:") || path.StartsWith("file:"))
            {
                return path;
            }

            return new Uri(path).AbsoluteUri;
        }

        private static string StreamingAssetUrl(string relativePath)
        {
            var rel = Normalize(relativePath);

            // Editor Assets/Levels ten okur, Build StreamingAssets ten okur.
#if UNITY_EDITOR
            var basePath = LevelsRoot.Replace("\\", "/");
#else
            var basePath = Application.streamingAssetsPath.Replace("\\", "/");
#endif

            return ToUri($"{basePath}/{rel}");
        }

        private static string PersistentAbsolute(string path)
        {
            var abs = Path.IsPathRooted(path) ?
                path :
                Path.Combine(Application.persistentDataPath, path);

            return abs;
        }

        private static async UniTask<byte[]> GetBytes(string url, IProgress<float> progress = null, CancellationToken ct = default)
        {
            using var req = UnityWebRequest.Get(url);
            await req.SendWebRequest().ToUniTask(progress: progress, cancellationToken: ct);
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new Exception(req.error);
            }

            return req.downloadHandler.data;
        }

        private static async UniTask<string> GetText(string url, IProgress<float> progress = null, CancellationToken ct = default)
        {
            using var req = UnityWebRequest.Get(url);
            await req.SendWebRequest().ToUniTask(progress: progress, cancellationToken: ct);
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new Exception(req.error);
            }

            return req.downloadHandler.text;
        }

        public static async UniTask<byte[]> GetResourceBytes(string resourcePathWithoutExtension, bool unloadAfterRead = true, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var req = Resources.LoadAsync<TextAsset>(resourcePathWithoutExtension);
            await req.ToUniTask(cancellationToken: ct);

            if (req.asset is not TextAsset ta)
            {
                throw new Exception($"TextAsset not found: {resourcePathWithoutExtension}");
            }

            var bytes = ta.bytes; // kucuk obejlerde ok
            if (unloadAfterRead)
            {
                Resources.UnloadAsset(ta);
            }

            return bytes;
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

        public static UniTask<string> GetStreamingText(string relativePath, IProgress<float> progress = null, CancellationToken ct = default)
            => GetText(StreamingAssetUrl(relativePath), progress, ct);

        public static UniTask<byte[]> GetStreamingBytes(string relativePath, IProgress<float> progress = null, CancellationToken ct = default)
            => GetBytes(StreamingAssetUrl(relativePath), progress, ct);

        public static async UniTask<string> ReadPersistentText(string relativeOrAbsolutePath, CancellationToken ct = default)
        {
            var abs = PersistentAbsolute(relativeOrAbsolutePath);
            return await File.ReadAllTextAsync(abs, ct);
        }

        public static async UniTask<byte[]> ReadPersistentBytes(string relativeOrAbsolutePath, CancellationToken ct = default)
        {
            var abs = PersistentAbsolute(relativeOrAbsolutePath);
            return await File.ReadAllBytesAsync(abs, ct);
        }

        public static async UniTask WritePersistentTextAtomic(string relativeOrAbsolutePath, string text, CancellationToken ct = default)
        {
            var abs = PersistentAbsolute(relativeOrAbsolutePath);
            Directory.CreateDirectory(Path.GetDirectoryName(abs)!);
            var tmp = abs + ".tmp";
            await File.WriteAllTextAsync(tmp, text, ct);

            if (File.Exists(abs))
            {
                File.Delete(abs);
            }

            File.Move(tmp, abs);
        }

        public static async UniTask WritePersistentBytesAtomic(string relativeOrAbsolutePath, byte[] bytes, CancellationToken ct = default)
        {
            var abs = PersistentAbsolute(relativeOrAbsolutePath);
            Directory.CreateDirectory(Path.GetDirectoryName(abs)!);
            var tmp = abs + ".tmp";
            await File.WriteAllBytesAsync(tmp, bytes, ct);

            if (File.Exists(abs))
            {
                File.Delete(abs);
            }

            File.Move(tmp, abs);
        }

        // STREAMING → PERSISTENT (atomic), büyük dosyalarda RAM'e almadan:
        public static async UniTask CopyStreamingToPersistentAtomic(
            string streamingRelative,
            string persistentRelative,
            bool appendJsonExt = true,
            IProgress<float> progress = null,
            CancellationToken ct = default)
        {
            var rel = Normalize(streamingRelative);
            if (appendJsonExt)
            {
                rel = WithJsonExt(rel);
            }

            var url = StreamingAssetUrl(rel);
            var dst = PersistentAbsolute(persistentRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);

            var tmp = dst + ".tmp";
            using var req = UnityWebRequest.Get(url);
            req.downloadHandler = new DownloadHandlerFile(tmp, true);
            await req.SendWebRequest().ToUniTask(progress: progress, cancellationToken: ct);

            if (req.result != UnityWebRequest.Result.Success)
            {
                if (File.Exists(tmp)) File.Delete(tmp);
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