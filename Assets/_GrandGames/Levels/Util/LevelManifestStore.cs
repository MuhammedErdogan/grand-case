// Assets/_GrandGames/Levels/Logic/Util/LevelManifestStore.cs
using System.IO;
using System.Threading;
using _GrandGames.Levels.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.Levels.Util
{
    public sealed class LevelManifestStore
    {
        private readonly string _folder; // "levels_manifest"

        public LevelManifestStore(string folder = "levels_manifest")
        {
            _folder = folder.Trim().Trim('/');
        }

        private string RelPath(int start, int end) => $"{_folder}/chunk_{start}_{end}.json";
        private static string Abs(string rel) => Path.Combine(Application.persistentDataPath, rel);

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

        public async UniTask SaveAsync(LevelChunkManifest m, CancellationToken ct)
        {
            var rel = RelPath(m.start, m.end);
            var abs = Abs(rel);
            Directory.CreateDirectory(Path.GetDirectoryName(abs)!);

            var tmp = abs + ".tmp";
            var json = JsonUtility.ToJson(m);
            await File.WriteAllTextAsync(tmp, json, ct);

            if (File.Exists(abs))
            {
                File.Delete(abs);
            }

            File.Move(tmp, abs);
        }
    }
}