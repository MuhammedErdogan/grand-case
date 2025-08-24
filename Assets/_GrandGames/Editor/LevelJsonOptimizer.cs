#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class LevelOptimizeMaskHexOutFolder
{
    [MenuItem("Tools/Levels/Optimize (maskHex → per-file, out folder, short keys)")]
    public static void Run()
    {
        var folder = EditorUtility.OpenFolderPanel("Select Levels Folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder)) return;

        // Output sibling folder: <SelectedFolderName>_maskHex_out
        var parent = Directory.GetParent(folder)?.FullName ?? folder;
        var outRoot = Path.Combine(parent, Path.GetFileName(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + "_maskHex_out");
        Directory.CreateDirectory(outRoot);

        int ok = 0, skip = 0, fail = 0, created = 0, overwritten = 0;
        var folderWithSep = folder.EndsWith(Path.DirectorySeparatorChar.ToString()) ?
            folder :
            folder + Path.DirectorySeparatorChar;

        foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(file);
            var lower = name.ToLowerInvariant();
            if (lower.EndsWith(".meta"))
            {
                skip++;
                continue;
            }

            if (file.StartsWith(outRoot))
            {
                skip++;
                continue; // don't reprocess outputs
            }

            string txt;
            try
            {
                var fi = new FileInfo(file);
                if (fi.Length == 0 || fi.Length > 10 * 1024 * 1024)
                {
                    skip++;
                    continue;
                }

                txt = File.ReadAllText(file, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(txt))
                {
                    skip++;
                    continue;
                }

                var first = txt.SkipWhile(char.IsWhiteSpace).FirstOrDefault();
                if (first != '{')
                {
                    skip++;
                    continue;
                }
            }
            catch
            {
                skip++;
                continue;
            }

            try
            {
                var src = JObject.Parse(txt);
                if (!TryBuildOptimized(src, out var optimized, out _))
                {
                    skip++;
                    continue;
                }

                // Preserve relative path
                var rel = file.StartsWith(folderWithSep) ?
                    file.Substring(folderWithSep.Length) :
                    name;
                var relDir = Path.GetDirectoryName(rel) ?? string.Empty;
                var outDir = Path.Combine(outRoot, relDir);
                Directory.CreateDirectory(outDir);

                // === OUTPUT FILENAME RULE ===
                // If source has .json → write BaseName + ".json"
                // Else → write original filename as-is (no extra .json)
                var srcExt = Path.GetExtension(rel);
                bool inputHasJsonExt = string.Equals(srcExt, ".json", StringComparison.OrdinalIgnoreCase);
                var outFileName = inputHasJsonExt ?
                    Path.GetFileNameWithoutExtension(rel) + ".json" :
                    Path.GetFileName(rel);

                var outPath = Path.Combine(outDir, outFileName);

                bool existedBefore = File.Exists(outPath);
                File.WriteAllText(outPath, optimized.ToString(Formatting.None), new UTF8Encoding(false));
                if (existedBefore) overwritten++;
                else created++;
                ok++;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LevelOptimizeMaskHexOutFolder] Skip (parse/convert fail): {file}\n{ex.Message}");
                fail++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[LevelOptimizeMaskHexOutFolder] Done. ok={ok}, skip={skip}, fail={fail}, created={created}, overwritten={overwritten}\nOut: {outRoot}");
    }

    private static bool TryBuildOptimized(JObject src, out JObject optimized, out int n)
    {
        optimized = null;
        n = 0;
        if (!TryGetMaskHex(src, out var maskHex, out n)) return false;

        // Short keys mapping: level→l, levelId→li, difficulty→d, gridSize→g, maskHex→mh
        var jo = new JObject
        {
            ["l"] = src["level"] ?? src["l"],
            ["li"] = src["levelId"] ?? src["li"],
            ["d"] = src["difficulty"] ?? src["d"],
            ["g"] = (JToken)(src["gridSize"] ?? src["g"] ?? new JValue(n)),
            ["mh"] = maskHex.ToLowerInvariant()
        };

        // Preserve other fields (except converted/verbose ones)
        foreach (var prop in src.Properties())
        {
            var name = prop.Name;
            if (name is "level" or "l" or "levelId" or "li" or "difficulty" or "d" or "gridSize" or "g" or "maskHex" or "mh" or "board" or "rows" or "boardStr")
                continue;
            if (jo[name] == null) jo[name] = prop.Value.DeepClone();
        }

        optimized = jo;
        return true;
    }

    // board/rows/boardStr/mh/maskHex sırasıyla dener; gridSize yoksa mümkünse türetir.
    private static bool TryGetMaskHex(JObject jo, out string maskHex, out int n)
    {
        maskHex = null;
        n = jo.Value<int?>("gridSize") ?? jo.Value<int?>("g") ?? 0;

        if (jo["board"] is JArray board && TryMaskFromBoard(board, ref n, out maskHex)) return true;
        if (jo["rows"] is JArray rows && TryMaskFromRows(rows, ref n, out maskHex)) return true;
        if (jo["boardStr"] is JValue bs && bs.Type == JTokenType.String && TryMaskFromFlatString((string)bs!, ref n, out maskHex)) return true;

        var mhTok = jo["mh"] ?? jo["maskHex"];
        if (mhTok is JValue mh && mh.Type == JTokenType.String)
        {
            var hex = ((string)mh!).Trim();
            if (TryInferNFromMaskHex(hex, ref n))
            {
                maskHex = hex.ToLowerInvariant();
                return true;
            }
        }

        return false;
    }

    private static bool TryMaskFromBoard(JArray board, ref int n, out string maskHex)
    {
        maskHex = null;
        if (board.Count == 0) return false;
        int rows = board.Count;
        int cols = (board[0] as JArray)?.Count ?? 0;
        if (rows <= 0 || cols <= 0 || rows != cols) return false;
        if (n <= 0) n = cols;
        if (n != cols) return false;

        var bits = new List<bool>(n * n);
        for (int r = 0; r < n; r++)
        {
            if (board[r] is not JArray row || row.Count != n) return false;
            for (int c = 0; c < n; c++) bits.Add((row[c]?.Value<string>()) == "x");
        }

        maskHex = PackBitsToHex(bits, n * n);
        return true;
    }

    private static bool TryMaskFromRows(JArray rows, ref int n, out string maskHex)
    {
        maskHex = null;
        if (rows.Count == 0) return false;
        var first = rows[0]?.Value<string>() ?? string.Empty;
        int cols = first.Length;
        if (cols == 0) return false;
        if (n <= 0) n = cols;
        if (rows.Count != n || cols != n) return false;

        var bits = new List<bool>(n * n);
        for (int r = 0; r < n; r++)
        {
            var s = rows[r]?.Value<string>() ?? string.Empty;
            if (s.Length != n) return false;
            for (int c = 0; c < n; c++) bits.Add(s[c] == 'x');
        }

        maskHex = PackBitsToHex(bits, n * n);
        return true;
    }

    private static bool TryMaskFromFlatString(string s, ref int n, out string maskHex)
    {
        maskHex = null;
        if (string.IsNullOrEmpty(s)) return false;
        if (n <= 0)
        {
            var len = s.Length;
            var root = (int)Math.Round(Math.Sqrt(len));
            if (root * root != len) return false;
            n = root;
        }

        if (s.Length != n * n) return false;

        var bits = new List<bool>(s.Length);
        foreach (var ch in s) bits.Add(ch == 'x');
        maskHex = PackBitsToHex(bits, s.Length);
        return true;
    }

    private static bool TryInferNFromMaskHex(string hex, ref int n)
    {
        if (string.IsNullOrEmpty(hex) || (hex.Length % 2) != 0) return false;
        int bits = (hex.Length / 2) * 8;
        int root = (int)Math.Round(Math.Sqrt(bits));
        if (root * root != bits) return false;
        if (n <= 0) n = root;
        else if (n != root) return false;
        return true;
    }

    /// row-major bits (x=1/o=0) → big-endian hex. Top-left = MSB.
    private static string PackBitsToHex(IReadOnlyList<bool> bits, int totalBits)
    {
        int byteCount = (totalBits + 7) / 8;
        var bytes = new byte[byteCount];
        for (int i = 0; i < totalBits; i++)
        {
            if (!bits[i]) continue;
            int inv = totalBits - 1 - i; // MSB-first
            int byteIdx = (byteCount - 1) - (inv / 8); // big-endian
            int bitInByte = inv % 8; // 0..7
            bytes[byteIdx] |= (byte)(1 << bitInByte);
        }

        var sb = new StringBuilder(byteCount * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
#endif