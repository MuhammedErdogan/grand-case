using System;
using System.Threading;
using _GrandGames.GameModules.Level.Domain;
using _GrandGames.GameModules.Level.Util;
using _GrandGames.Util;
using Cysharp.Threading.Tasks;

namespace _GrandGames.GameModules.Level.Source
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
            var json = await LocalFileHelper.GetResourceText(resPath, unloadAfterRead: true, ct: ct);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var levelData = LevelJson.Parse(json);

            return levelData;
        }
    }
}