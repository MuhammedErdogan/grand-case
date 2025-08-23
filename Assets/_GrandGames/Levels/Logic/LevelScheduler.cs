using System;
using System.Threading;

namespace _GrandGames.Levels.Logic
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 0-500 levels
    /// 0 --> 10
    /// 10 --> 50 ilk 10 leveli discard et, zatne oyandim
    /// Algoritma: hangi dilimdeyim? bulundugum dilimdeki next remote 50 leveli al ancak oynaigim kisma kadar olani discard et
    /// 25 --> 30 oynadim, oyleyse 25'lik dilimdeyim, 30'a kadar discard et, ilk 50 zaten indilirilmis olabilir o halde indirecegim aralik 50-75
    /// Algorithm: [oynadigim level,((bulundugum dilim + 2) * 25)] rangeinde zaten indirilmiş olanlar haric
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Serializable]
    public class LevelScheduler : IDisposable
    {
        private readonly SemaphoreSlim _concurrency = new(2);
        private readonly CancellationTokenSource _cts = new();

        public void Dispose()
        {
        }
    }
}