using System;
using System.IO;
using System.Threading;
using _GrandGames.GameModules.Level.Domain;
using _GrandGames.GameModules.Level.Util;
using _GrandGames.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.GameModules.Level.Source
{
    [Serializable]
    public sealed class CacheSource : ILevelSource
    {
        private readonly string _persistentFolder; //levels

        public CacheSource(string persistentFolder = "levels")
        {
            _persistentFolder = persistentFolder.Trim().TrimStart('/');
        }

        public async UniTask<LevelData> TryGetAsync(int level, CancellationToken ct)
        {
            var rel = $"{_persistentFolder}/level_{level}.json";

            try
            {
                var json = await LocalFileHelper.ReadPersistentText(rel, ct);
                var data = LevelJson.Parse(json);
                if (data == null)
                {
                    Debug.LogWarning($"Level {level} JSON parse null.");
                }

                return data;
            }
            catch
            {
                return null;
            }
        }

        public async UniTask DeleteFromCacheAsync(int lvl, CancellationToken ct = default)
        {
            var rel = $"{_persistentFolder}/level_{lvl}.json";
            var abs = Path.Combine(Application.persistentDataPath, rel);

            await UniTask.SwitchToThreadPool();
            try
            {
                if (File.Exists(abs))
                {
                    File.Delete(abs);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CacheSource] DeleteFromCacheAsync L{lvl} error: {e.Message}");
            }
            finally
            {
                await UniTask.SwitchToMainThread();
            }
        }
    }
}