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
            if (_currentLevel?.level is { } curLvl && curLvl == CurrentLevel)
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

            Debug.Log($"[LevelService] 2 Returning cached level data for level {CurrentLevel}");

            levelData = await _remoteSource.TryGetAsync(level, ct);
            if (levelData != null)
            {
                Debug.Log($"[LevelService] 3 Returning level data for level {level} from RemoteSource");
                return levelData;
            }

            Debug.Log($"[LevelService] 4 Returning level data for level {level} from ResourcesSource");

            levelData = await _resourcesSource.TryGetAsync(level, ct);

            Debug.Log($"[LevelService] 5 Returning level data for level {level} from ResourcesSource");

            return levelData;
        }

        public void IncrementLevel()
        {
            var current = GetCurrentLevelIndex();
            UserSaveHelper.SaveInt(CURRENT_LEVEL_KEY, current + 1);
        }

        public async UniTask<Difficulty> PrepareCurrentLevel(CancellationToken ct)
        {
            _currentLevel = await GetCurrentLevelData(ct);

            if (_currentLevel?.level is { } curLvl && curLvl == CurrentLevel)
            {
                return _currentLevel.difficulty;
            }

            return Difficulty.Medium;
        }

        //TODO: remove these test methods
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
    }
}