using System;

namespace _GrandGames.GameModules.Level.Domain
{
    [Serializable]
    public sealed class LevelChunkManifest
    {
        public int start; // exclusive
        public int end; // inclusive
        public bool[] ok; // 50 lenght: ok[i] => (start + i) downloaded

        public static LevelChunkManifest Create(int chunkStart)
        {
            return new LevelChunkManifest
            {
                start = chunkStart,
                end = chunkStart + 49,
                ok = new bool[50]
            };
        }

        public bool IsComplete(int startLevel)
        {
            var a = ok;

            startLevel -= start - 1; //to 0 based index

            if (a is not { Length: 50 })
            {
                return false;
            }

            for (int i = startLevel; i < 50; i++)
            {
                if (!a[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}