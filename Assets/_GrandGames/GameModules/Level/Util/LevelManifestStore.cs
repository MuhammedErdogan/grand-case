using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _GrandGames.GameModules.Level.Domain;

namespace _GrandGames.GameModules.Level.Util
{
    public sealed class LevelManifestStore
    {
        private readonly string _folder;

        public LevelManifestStore(string folder = "levels_manifest")
        {
            _folder = folder.Trim().Trim('/');
        }

        private string RelPath(int start, int end) => $"{_folder}/chunk_{start}_{end}.json";
        private static string Abs(string rel) => Path.Combine(Application.persistentDataPath, rel);

        // === SemaphoreSlim for each file ===
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new();

        private static SemaphoreSlim GetLock(string absPath) =>
            FileLocks.GetOrAdd(absPath, _ => new SemaphoreSlim(1, 1));

        public async UniTask<LevelChunkManifest> LoadAsync(int start, int end, CancellationToken ct)
        {
            var rel = RelPath(start, end);
            var abs = Abs(rel);
            if (!File.Exists(abs))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(abs, ct);
            return string.IsNullOrWhiteSpace(json) ?
                null :
                JsonUtility.FromJson<LevelChunkManifest>(json);
        }

        public async UniTask DeleteAsync(int start, int end, CancellationToken ct)
        {
            var rel = RelPath(start, end);
            var abs = Abs(rel);
            var dir = Path.GetDirectoryName(abs)!;
            Directory.CreateDirectory(dir);

            var gate = GetLock(abs);
            await gate.WaitAsync(ct);
            try
            {
                if (!File.Exists(abs))
                {
                    return;
                }

                try
                {
                    foreach (var tmp in Directory.EnumerateFiles(dir, Path.GetFileName(abs) + ".*.tmp"))
                    {
                        File.Delete(tmp);
                    }
                }
                catch
                {
                    Debug.LogWarning($"[LevelManifestStore] Cleanup temp files failed: {dir}");
                }


                File.Delete(abs);
            }
            finally
            {
                gate.Release();
            }
        }
        
        public async UniTask UpdateBitAsync(int start, int end, int level, CancellationToken ct)
        {
            var rel = RelPath(start, end);
            var abs = Abs(rel);
            Directory.CreateDirectory(Path.GetDirectoryName(abs)!);

            var gate = GetLock(abs);
            await gate.WaitAsync(ct);
            try
            {
                LevelChunkManifest m;
                if (File.Exists(abs))
                {
                    var jsonOld = await File.ReadAllTextAsync(abs, ct);
                    m = string.IsNullOrWhiteSpace(jsonOld) ?
                        LevelChunkManifest.Create(start) :
                        JsonUtility.FromJson<LevelChunkManifest>(jsonOld) ?? LevelChunkManifest.Create(start);
                }
                else
                {
                    m = LevelChunkManifest.Create(start);
                }

                var idx = level - m.start;
                if (idx is >= 0 and < 50)
                {
                    m.ok[idx] = true;
                }

                var tmp = abs + "." + Guid.NewGuid().ToString("N") + ".tmp";
                var json = JsonUtility.ToJson(m);
                await File.WriteAllTextAsync(tmp, json, ct);

                try
                {
                    if (File.Exists(abs))
                    {
                        File.Replace(tmp, abs, null);
                    }
                    else
                    {
                        File.Move(tmp, abs);
                    }
                }
                catch
                {
                    File.Copy(tmp, abs, true);
                    File.Delete(tmp);
                }
            }
            finally
            {
                gate.Release();
            }
        }
    }
}