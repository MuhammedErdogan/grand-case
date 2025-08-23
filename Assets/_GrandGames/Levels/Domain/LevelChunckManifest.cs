// Assets/_GrandGames/Levels/Logic/Domain/LevelChunkManifest.cs
using System;

namespace _GrandGames.Levels.Domain
{
    [Serializable]
    public sealed class LevelChunkManifest
    {
        public int start; // haric
        public int end; // dahil
        public bool[] ok; // 50 uzunluk: ok[i] => (start + i) indirildi/kalici

        public static LevelChunkManifest Create(int chunkStart)
        {
            return new LevelChunkManifest
            {
                start = chunkStart,
                end = chunkStart + 49,
                ok = new bool[50]
            };
        }

        public bool IsComplete()
        {
            var a = ok;
            if (a is not { Length: 50 })
            {
                return false;
            }

            for (var i = 0; i < 50; i++)
            {
                if (!a[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int IndexOf(int level) => level - start; // aralıkta olduğu varsayımıyla
    }
}