using System;
using System.Threading;
using _GrandGames.GameModules.Level.Domain;
using _GrandGames.GameModules.Level.Source;
using _GrandGames.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.GameModules.Level
{
    [Serializable]
    public class LevelService
    {
        [SerializeField] private ResourcesSource _resourcesSource = new();
        [SerializeField] private CacheSource _cacheSource = new();

        [SerializeField] private RemoteSource _remoteSource = new();
        public RemoteSource RemoteSource => _remoteSource;

        private const string CURRENT_LEVEL_KEY = "CurrentLevel";

        private LevelData _currentLevel;

        public int GetCurrentLevelIndex() => UserSaveHelper.LoadInt(CURRENT_LEVEL_KEY);
        public int CurrentLevel => GetCurrentLevelIndex() + 1; //index 0-based, levels 1-based

        public async UniTask<LevelData> GetCurrentLevelData(CancellationToken ct)
        {
            if (_currentLevel?.Level is { } curLvl && curLvl == CurrentLevel)
            {
                Debug.Log($"[LevelService] 0 Returning cached current level data for level {CurrentLevel}");
                return _currentLevel;
            }

            var level = CurrentLevel;
            var levelData = await _cacheSource.TryGetAsync(level, ct);
            if (levelData != null)
            {
                Debug.Log($"[LevelService] 1 Returning cached level data for level {level} from CacheSource");
                return levelData;
            }

            if (Application.internetReachability is not NetworkReachability.NotReachable) //for simulate remote source offline
            {
                levelData = await _remoteSource.TryGetAsync(level, ct);
                if (levelData != null)
                {
                    Debug.Log($"[LevelService] 3 Returning level data for level {level} from RemoteSource");
                    return levelData;
                }
            }

            levelData = await _resourcesSource.TryGetAsync(level, ct);

            Debug.Log($"[LevelService] 5 Returning level data for level {level} from ResourcesSource");

            return levelData;
        }

        public void IncrementLevel()
        {
            var current = GetCurrentLevelIndex();
            var level = (current + 1) % 500;

            UserSaveHelper.SaveInt(CURRENT_LEVEL_KEY, level);
        }

        public async UniTask<Difficulty> PrepareCurrentLevel(CancellationToken ct)
        {
            _currentLevel = await GetCurrentLevelData(ct);

            if (_currentLevel?.Level is { } curLvl && curLvl == CurrentLevel)
            {
                return _currentLevel.Difficulty;
            }

            return Difficulty.Medium;
        }

        public async void ClearLastLevelCache(CancellationToken ct = default)
        {
            var lvl = CurrentLevel;
            try
            {
                await _cacheSource.DeleteFromCacheAsync(lvl, ct);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LevelService] Cache delete failed L{lvl}: {e.Message}");
            }
        }

#if UNITY_EDITOR

        // For testing purposes only
        public void GetFromRemote()
        {
            _remoteSource.TryGetAsync(1, default).Forget();
        }

        public void GetFromCache()
        {
            _cacheSource.TryGetAsync(1, default).Forget();
        }

        public void GetFromResources()
        {
            _resourcesSource.TryGetAsync(1, default).Forget();
        }

#endif
    }
}