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

        /// Oyuncu bir level bitirdiginde çagir.
        /// - icinde bulunulan pencereyi tamamla (manifest’e gore eksikleri indir)
        public async UniTask CheckLevelSchedule(int playedLevel, RemoteSource rs)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("[LevelScheduler] Offline; prefetch skipped.");
                return;
            }

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

            //TODO: previous manifest silinebilir
            //TODO: eski leveller silinebilir
            await CleanupPreviousAsync(manifestPreviously, manifest, rs, ct);

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
                _ = DownloadAndMark(lvl, chunkStart, chunkEnd, rs, ct);
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

        private async UniTask CleanupPreviousAsync(LevelChunkManifest prev, LevelChunkManifest current, RemoteSource rs, CancellationToken ct)
        {
            if (prev is null)
            {
                return;
            }

            //TODO: logic implement
        }

        private async UniTaskVoid DownloadAndMark(int lvl, int chunkStart, int chunkEnd, RemoteSource rs, CancellationToken ct)
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