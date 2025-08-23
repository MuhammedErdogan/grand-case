using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _GrandGames.Levels.Domain;
using _GrandGames.Levels.Source;
using _GrandGames.Util;

namespace _GrandGames.Levels
{
    [Serializable]
    public class LevelService
    {
        [SerializeField] private ResourcesSource _resourcesSource;
        [SerializeField] private CacheSource _cacheSource;
        [SerializeField] private RemoteSource _remoteSource;

        private const string CURRENT_LEVEL_KEY = "CurrentLevel";

        public int GetCurrentLevelIndex() => UserSaveHelper.LoadInt(CURRENT_LEVEL_KEY);

        private void Awake()
        {
            _resourcesSource = new ResourcesSource();
            _cacheSource = new CacheSource();
            _remoteSource = new RemoteSource();
        }

        public async UniTask<LevelData> GetLevelData(int level, CancellationToken ct)
        {
            var levelData = await _cacheSource.TryGetAsync(level, ct);
            if (levelData != null)
            {
                return levelData;
            }

            levelData = await _remoteSource.TryGetAsync(level, ct);
            if (levelData != null)
            {
                return levelData;
            }

            levelData = await _resourcesSource.TryGetAsync(level, ct);

            return levelData;
        }

        public void GetFromRemote()
        {
            Awake();
            _remoteSource.TryGetAsync(1, default).Forget();
        }

        public void GetFromCache()
        {
            Awake();
            _cacheSource.TryGetAsync(1, default).Forget();
        }

        public void GetFromResources()
        {
            Awake();
            _resourcesSource.TryGetAsync(1, default).Forget();
        }
    }
}