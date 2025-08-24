using System;
using System.Threading;
using _GrandGames.GameModules.Level.Domain;
using _GrandGames.GameModules.Level.Util;
using _GrandGames.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.GameModules.Level.Source
{
    [Serializable]
    public sealed class RemoteSource : ILevelSource
    {
        private readonly string _streamingFolder; // "levels_1_500"
        private readonly string _persistentFolder; // "levels"

        public RemoteSource(string streamingFolder = "levels_1_500", string persistentFolder = "levels")
        {
            _streamingFolder = streamingFolder.Trim().TrimStart('/');
            _persistentFolder = persistentFolder.Trim().TrimStart('/');
        }

        public async UniTask<LevelData> TryGetAsync(int level, CancellationToken ct)
        {
            try
            {
                var streamingRel = $"{_streamingFolder}/level_{level}_updated"; // .json opsiyonel
                var persistentRel = $"{_persistentFolder}/level_{level}.json";

                await LocalFileHelper.CopyStreamingToPersistentAtomic(
                    streamingRelative: streamingRel,
                    persistentRelative: persistentRel,
                    ct: ct);

                var json = await LocalFileHelper.ReadPersistentText(persistentRel, ct);
                return string.IsNullOrWhiteSpace(json) ?
                    null :
                    LevelJson.Parse(json);
            }
            catch
            {
                return null; // streaming'de yoksa/hata varsa diğer kaynağa düş
            }
        }

        public async UniTask SaveToCacheAsync(int level, CancellationToken ct)
        {
            try
            {
                var streamingRel = $"{_streamingFolder}/level_{level}_updated"; // .json opsiyonel
                var persistentRel = $"{_persistentFolder}/level_{level}.json";

                await LocalFileHelper.CopyStreamingToPersistentAtomic(
                    streamingRelative: streamingRel,
                    persistentRelative: persistentRel,
                    ct: ct);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}