using System;
using _GrandGames.Levels.Logic.Domain;
using _GrandGames.Levels.Logic.Source;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.Levels.Logic
{
    public class LevelService : MonoBehaviour
    {
        [SerializeField] private ResourcesSource _resourcesSource;
        [SerializeField] private CacheSource _cacheSource;
        [SerializeField] private RemoteSource _remoteSource;

        private void Awake()
        {
            _resourcesSource = new ResourcesSource();
            _cacheSource = new CacheSource();
            _remoteSource = new RemoteSource();
        }

        public async UniTask<LevelData> GetLevelData(int level)
        {
            var onDestroyToken = this.GetCancellationTokenOnDestroy();

            var levelData = await _cacheSource.TryGetAsync(level, onDestroyToken);
            if (levelData != null)
            {
                return levelData;
            }

            levelData = await _remoteSource.TryGetAsync(level, onDestroyToken);
            if (levelData != null)
            {
                return levelData;
            }

            levelData = await _resourcesSource.TryGetAsync(level, onDestroyToken);

            return levelData;
        }
    }
}