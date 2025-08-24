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
        public async void CheckLevelSchedule(int playedLevel, RemoteSource rs)
        {
            var ct = _cts.Token;

            //[oynadigim level,((bulundugum dilim + 2) * 25)] 
            var currentChunk = Mathf.FloorToInt(playedLevel / 25f);
            var currentStartExclusive = playedLevel + 1;
            var chunkStart = currentChunk * 25 + 1;
            var chunkEnd = (currentChunk + 2) * 25;

            LevelChunkManifest manifestPreviously = null;
            if (currentChunk > 0)
            {
                var previousStartChunk = Mathf.Max(currentChunk - 2, 0);
                var previousEndChunk = previousStartChunk == 0 ?
                    currentChunk + 1 :
                    currentChunk;

                var previousStartIndex = previousStartChunk * 25 + 1;
                var previousEndIndex = previousEndChunk * 25;

                manifestPreviously = await _manifestStore.LoadAsync(previousStartIndex, previousEndIndex, ct);
            }

            var manifest = await _manifestStore.LoadAsync(chunkStart, chunkEnd, ct) ?? LevelChunkManifest.Create(chunkStart);

            CheckIsPreviouslyDownloaded(manifestPreviously, currentStartExclusive, manifest);

            if (manifest.IsComplete(currentStartExclusive - 1))
            {
                return;
            }

            for (var lvl = currentStartExclusive; lvl <= chunkEnd; lvl++)
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
                _ = DownloadAndMark(lvl, chunkStart, chunkEnd, rs, manifest, ct);
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

            //TODO: previous manifest silinebilir
            //TODO: eski leveller silinebilir
        }

        private async UniTaskVoid DownloadAndMark(int lvl, int chunkStart, int chunkEnd, RemoteSource rs, LevelChunkManifest manifest, CancellationToken ct)
        {
            try
            {
                await rs.SaveToCacheAsync(lvl, ct);

                await _manifestStore.UpdateBitAsync(chunkStart, chunkEnd, lvl, ct); // merge-safe
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