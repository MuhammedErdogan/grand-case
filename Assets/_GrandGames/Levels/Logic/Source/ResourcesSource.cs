using System;
using System.Threading;
using _GrandGames.Levels.Logic.Domain;
using _GrandGames.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.Levels.Logic.Source
{
    [Serializable]
    public sealed class ResourcesSource : ILevelSource
    {
        // Resources path: "resources_levels_1_500/level_1"

        private readonly string _resourceFolder;

        public ResourcesSource(string resourceFolder = "resources_levels_1_500")
        {
            _resourceFolder = resourceFolder.Trim().TrimStart('/');
        }

        public async UniTask<LevelData> TryGetAsync(int level, CancellationToken ct)
        {
            var resPath = $"{_resourceFolder}/level_{level}";
            var text = await LocalFileHelper.GetResourceText(resPath, unloadAfterRead: true, ct: ct);
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var levelData = JsonUtility.FromJson<LevelData>(text);

            return levelData;
        }
    }
}