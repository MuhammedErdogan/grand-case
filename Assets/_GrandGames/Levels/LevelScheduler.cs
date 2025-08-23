using System;
using System.Threading;
using _GrandGames.Levels.Domain;
using _GrandGames.Levels.Source;
using _GrandGames.Levels.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.Levels
{
    [Serializable]
    public class LevelScheduler : IDisposable
    {
        private readonly SemaphoreSlim _concurrency = new(2);
        private readonly CancellationTokenSource _cts = new();
        private readonly LevelManifestStore _manifestStore = new();

        /// Oyuncu bir level bitirdiğinde çağır.
        /// - İçinde bulunulan pencereyi tamamla (manifest’e göre eksikleri indir)
        public async void OnLevelFinished(int playedLevel, RemoteSource rs)
        {
            var ct = _cts.Token;

            //[oynadigim level,((bulundugum dilim + 2) * 25)] 
            var currentChunk = Mathf.FloorToInt(playedLevel / 25f);
            var currentStartExclusive = playedLevel + 1;
            var chunkStart = (currentChunk) * 25;
            var endInclusive = (currentChunk + 2) * 25;

            //var manifest = await _manifestStore.LoadAsync(currentStartExclusive, endInclusive, ct) ?? LevelChunkManifest.Create(chunkStart);

            // if (manifest.IsComplete())
            // {
            //     return;
            // }

            for (var lvl = currentStartExclusive; lvl <= endInclusive; lvl++)
            {
                var idx = lvl - currentStartExclusive;
                if (idx is < 0 or >= 50)
                {
                    continue;
                }

                // if (manifest.ok[idx])
                // {
                //     continue;
                // }

                await _concurrency.WaitAsync(ct);
                _ = DownloadAndMark(lvl, rs, ct);
            }
        }

        private async UniTaskVoid DownloadAndMark(int level, RemoteSource rs, CancellationToken ct)
        {
            try
            {
                await rs.SaveToCacheAsync(level, ct);

                // var idx = manifest.IndexOf(level);
                // if (idx is >= 0 and < 50)
                // {
                //     manifest.ok[idx] = true;
                //     await _manifestStore.SaveAsync(manifest, ct); // atomic update
                // }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Prefetch {level}: {e.Message}");
            }
            finally
            {
                _concurrency.Release();
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _concurrency.Dispose();
        }
    }
}