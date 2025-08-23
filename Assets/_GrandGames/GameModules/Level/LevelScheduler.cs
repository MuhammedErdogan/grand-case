using System;
using System.Threading;
using _GrandGames.GameModules.Level.Domain;
using _GrandGames.GameModules.Level.Source;
using _GrandGames.GameModules.Level.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.GameModules.Level
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
            var chunkStart = currentChunk * 25 + 1;
            var endInclusive = (currentChunk + 2) * 25;

            LevelChunkManifest manifestPreviously = null;
            if (currentChunk > 0)
            {
                var previousChunkStart = (currentChunk - 1) * 25 + 1;
                var previousChunkEnd = (currentChunk + 1) * 25;
                manifestPreviously = await _manifestStore.LoadAsync(previousChunkStart, previousChunkEnd, ct);
            }

            var manifest = await _manifestStore.LoadAsync(chunkStart, endInclusive, ct) ?? LevelChunkManifest.Create(chunkStart);

            CheckIsPreviouslyDownloaded(manifestPreviously, currentStartExclusive, manifest);

            if (manifest.IsComplete() || manifest.IsCompleteFrom(currentStartExclusive - 1))
            {
                return;
            }

            for (var lvl = currentStartExclusive; lvl <= endInclusive; lvl++)
            {
                var idx = lvl - currentStartExclusive;
                if (idx is < 0 or >= 50)
                {
                    continue;
                }

                if (manifest.ok[idx])
                {
                    continue;
                }

                await _concurrency.WaitAsync(ct);
                _ = DownloadAndMark(lvl, rs, manifest, ct);
            }
        }

        private static void CheckIsPreviouslyDownloaded(LevelChunkManifest manifestPreviously, int currentStartExclusive, LevelChunkManifest manifest)
        {
            if (manifestPreviously is null)
            {
                return;
            }

            var checkForContinueStart = Math.Max(currentStartExclusive - 25, manifestPreviously.start);
            for (var lvl = checkForContinueStart; lvl <= manifestPreviously.end; lvl++)
            {
                var idxPrev = lvl - manifestPreviously.start;
                var idxCurr = lvl - manifest.start;
                if (idxPrev is < 0 or >= 50 || idxCurr is < 0 or >= 50)
                {
                    continue;
                }

                if (manifestPreviously.ok[idxPrev])
                {
                    manifest.ok[idxCurr] = true;
                }
            }
        }

        private async UniTaskVoid DownloadAndMark(int lvl, RemoteSource rs, LevelChunkManifest manifest, CancellationToken ct)
        {
            try
            {
                await rs.SaveToCacheAsync(lvl, ct);

                var idx = lvl - manifest.start;
                if (idx is < 0 or >= 50)
                {
                    return;
                }

                manifest.ok[idx] = true;
                await _manifestStore.SaveAsync(manifest, ct); // atomic update
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Prefetch {lvl}: {e.Message}");
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