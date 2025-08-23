using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _GrandGames.Levels.Logic.Domain;
using _GrandGames.Levels.Logic.Util;

namespace _GrandGames.Levels.Logic.Source
{
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