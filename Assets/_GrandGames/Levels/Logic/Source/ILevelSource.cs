using System.Threading;
using _GrandGames.Levels.Logic.Domain;
using Cysharp.Threading.Tasks;

namespace _GrandGames.Levels.Logic.Source
{
    public interface ILevelSource
    {
        UniTask<LevelData> TryGetAsync(int level, CancellationToken ct);
    }
}