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

        private RemoteSource _remoteSource;

        private const int LEVEL_CHECK_INTERVAL = 25;
        private const int LEVEL_CHUNK_SIZE = 50;

        public void BindReferences(RemoteSource rs)
        {
            _remoteSource = rs;
        }

        /// call this method when a level is completed and on game start
        /// complete current chunk
        public async UniTask CheckLevelSchedule(int playedLevel)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) //for simulate remote source offline
            {
                Debug.Log("[LevelScheduler] Offline; prefetch skipped.");
                return;
            }

            var ct = _cts.Token;

            //[oynadigim level,((bulundugum dilim + 2) * 25)] 
            var currentChunk = Mathf.FloorToInt(playedLevel / 25f);
            var currentStartExclusive = playedLevel + 1;
            var chunkStart = currentChunk * LEVEL_CHECK_INTERVAL + 1;
            var chunkEnd = (currentChunk + 2) * LEVEL_CHECK_INTERVAL;

            LevelChunkManifest manifestPreviously = null;
            if (currentChunk > 0)
            {
                var previousStartChunk = Mathf.Max(currentChunk - 2, 0);
                var previousEndChunk = previousStartChunk == 0 ?
                    currentChunk + 1 :
                    currentChunk;

                var previousStartIndex = previousStartChunk * LEVEL_CHECK_INTERVAL + 1;
                var previousEndIndex = previousEndChunk * LEVEL_CHECK_INTERVAL;

                manifestPreviously = await _manifestStore.LoadAsync(previousStartIndex, previousEndIndex, ct);
            }

            var manifest = await _manifestStore.LoadAsync(chunkStart, chunkEnd, ct) ?? LevelChunkManifest.Create(chunkStart);

            CheckIsPreviouslyDownloaded(manifestPreviously, currentStartExclusive, manifest);

            await CleanupPreviousAsync(manifestPreviously, ct);

            if (manifest.IsComplete(currentStartExclusive - 1))
            {
                return;
            }

            for (var lvl = currentStartExclusive; lvl <= chunkEnd; lvl++)
            {
                var idx = lvl - currentStartExclusive;
                if (idx is < 0 or >= LEVEL_CHUNK_SIZE)
                {
                    continue;
                }

                if (manifest.ok[idx])
                {
                    continue;
                }

                await _concurrency.WaitAsync(ct);
                _ = DownloadAndMark(lvl, chunkStart, chunkEnd, ct);
            }
        }

        private static void CheckIsPreviouslyDownloaded(LevelChunkManifest manifestPreviously, int currentStartExclusive, LevelChunkManifest manifest)
        {
            if (manifestPreviously is null)
            {
                return;
            }

            var checkForContinueStart = Math.Max(currentStartExclusive - LEVEL_CHECK_INTERVAL, manifestPreviously.start);
            for (var lvl = checkForContinueStart; lvl <= manifestPreviously.end; lvl++)
            {
                var idxPrev = lvl - manifestPreviously.start;
                var idxCurr = lvl - manifest.start;
                if (idxPrev is < 0 or >= LEVEL_CHUNK_SIZE || idxCurr is < 0 or >= LEVEL_CHUNK_SIZE)
                {
                    continue;
                }

                if (manifestPreviously.ok[idxPrev])
                {
                    manifest.ok[idxCurr] = true;
                }
            }
        }

        private async UniTask CleanupPreviousAsync(LevelChunkManifest prev, CancellationToken ct)
        {
            if (prev is null)
            {
                return;
            }

            try
            {
                await _manifestStore.DeleteAsync(prev.start, prev.end, ct); // LevelManifestStore'da bu metod olmalı
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LevelScheduler] Manifest delete failed [{prev.start}-{prev.end}]: {e.Message}");
            }
        }

        private async UniTaskVoid DownloadAndMark(int lvl, int chunkStart, int chunkEnd, CancellationToken ct)
        {
            try
            {
                await _remoteSource.SaveToCacheAsync(lvl, ct);

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