using System.Threading;
using _GrandGames.GameModules.Level.Domain;
using Cysharp.Threading.Tasks;

namespace _GrandGames.GameModules.Level.Source
{
    public interface ILevelSource
    {
        UniTask<LevelData> TryGetAsync(int level, CancellationToken ct);
    }
}