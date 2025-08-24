using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using _GrandGames.GameModules.Level.Domain;

namespace _GrandGames.GameModules.Level.Util
{
    internal static class LevelJson
    {
        public static LevelData Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var o = JObject.Parse(json);

            if (!ReadOptimized(o, out var opt))
            {
                return null;
            }

            var ld = new LevelData
            {
                Level = opt.L,
                LevelId = opt.Li,
                Difficulty = opt.D,
                GridSize = opt.G,
                Board = BoardFromMaskHex(opt.Mh, opt.G)
            };

            return ld;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ReadOptimized(JObject o, out OptimizedLevelJson opt)
        {
            opt = null;
            // Short veya long anahtarları toleranslı oku
            var g = I(o, "g") ?? I(o, "gridSize");
            var mh = S(o, "mh") ?? S(o, "maskHex");
            if (g is null || string.IsNullOrWhiteSpace(mh)) return false;

            var dStr = S(o, "d") ?? S(o, "difficulty");
            var d = ParseDifficulty(dStr);

            opt = new OptimizedLevelJson
            {
                L = I(o, "l") ?? I(o, "level") ?? 0,
                Li = S(o, "li") ?? S(o, "levelId"),
                D = d,
                G = g.Value,
                Mh = mh.Trim().ToLowerInvariant()
            };
            return true;
        }

        // === helpers ===
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Difficulty ParseDifficulty(string s)
        {
            if (string.IsNullOrEmpty(s)) return Difficulty.Medium;
            return Enum.TryParse<Difficulty>(s, true, out var d) ?
                d :
                Difficulty.Medium;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string S(JObject o, string key) =>
            o[key]?.Type == JTokenType.String ?
                (string)o[key] :
                o[key]?.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int? I(JObject o, string key)
        {
            var t = o[key];
            if (t == null)
            {
                return null;
            }

            return t.Type switch
            {
                JTokenType.Integer => (int)t,
                JTokenType.String when int.TryParse((string)t, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) => v,
                _ => null
            };
        }

        // Bit duzeni: row-major, (0,0)
        private static string[][] BoardFromMaskHex(string mh, int g)
        {
            var bytes = HexToBytes(mh);
            var total = g * g;
            var needBytes = (total + 7) / 8;

            // Fazla byte varsa bastan kirp
            if (bytes.Length > needBytes)
            {
                var start = bytes.Length - needBytes;
                var trimmed = new byte[needBytes];
                Buffer.BlockCopy(bytes, start, trimmed, 0, needBytes);
                bytes = trimmed;
            }

            if (bytes.Length * 8 < total)
            {
                return null;
            }

            var board = NewBoard(g);
            var byteCount = bytes.Length;

            for (var i = 0; i < total; i++)
            {
                var inv = total - 1 - i;
                var byteIdx = byteCount - 1 - inv / 8;
                var bitInByte = inv % 8;
                var v = ((bytes[byteIdx] >> bitInByte) & 1) != 0;

                int r = i / g, c = i % g;
                board[r][c] = v ?
                    "x" :
                    "o";
            }

            return board;
        }

        private static string[][] NewBoard(int g)
        {
            var b = new string[g][];
            for (var r = 0; r < g; r++)
            {
                b[r] = new string[g];
            }

            return b;
        }

        private static byte[] HexToBytes(string hex)
        {
            hex = hex.Trim();
            if ((hex.Length & 1) == 1)
            {
                return null;
            }

            var len = hex.Length / 2;
            var bytes = new byte[len];
            for (var i = 0; i < len; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
    }
}