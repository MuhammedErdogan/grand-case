using System;
using System.Threading;
using _GrandGames.Levels.Domain;
using _GrandGames.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.Levels.Source
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
                return string.IsNullOrWhiteSpace(json) ?
                    null :
                    JsonUtility.FromJson<LevelData>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}