using System.Threading;
using _GrandGames.Levels.Domain;
using Cysharp.Threading.Tasks;

namespace _GrandGames.Levels.Source
{
    public interface ILevelSource
    {
        UniTask<LevelData> TryGetAsync(int level, CancellationToken ct);
    }
}