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
        private static string LevelsRoot
            => Path.Combine(Application.dataPath, "Levels");

        private static string StreamingAssetUrl(string relativePath)
        {
            //var basePath = Application.streamingAssetsPath; //if editor degilse buradan okuyacak ve build esnasinda asset post processor ile streaming assete kopyalayacak
            var basePath = LevelsRoot;
            if (relativePath.StartsWith("/"))
            {
                relativePath = relativePath[1..];
            }

            var path = basePath + "/" + relativePath;
            if (path.StartsWith("jar:") || path.StartsWith("file:"))
            {
                return path;
            }

            return new Uri(path).AbsoluteUri;
        }

        private static string PersistentDataUrl(string path)
        {
            var abs = Path.IsPathRooted(path) ?
                path :
                Path.Combine(Application.persistentDataPath, path);

            return new Uri(abs.Replace("\\", "/")).AbsoluteUri;
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

            var ta = req.asset as TextAsset;
            if (ta == null)
            {
                throw new Exception($"TextAsset not found: {resourcePathWithoutExtension}");
            }

            var bytes = ta.bytes;
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

            var ta = req.asset as TextAsset;
            if (ta == null)
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

        public static UniTask<string> GetPersistentText(string path, IProgress<float> progress = null, CancellationToken ct = default)
            => GetText(PersistentDataUrl(path), progress, ct);

        public static UniTask<byte[]> GetPersistentBytes(string path, IProgress<float> progress = null, CancellationToken ct = default)
            => GetBytes(PersistentDataUrl(path), progress, ct);
    }
}