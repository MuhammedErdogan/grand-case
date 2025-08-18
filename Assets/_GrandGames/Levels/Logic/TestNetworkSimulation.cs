using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TestNetworkSimulation : MonoBehaviour
{
    //local data path: Assets/_GrandGames/Resources/resources_levels_1_500
    //remote data path: Assets/_GrandGames/Levels/levels_1_500

    private static readonly Regex LevelNameRegex = new("^[a-zA-Z0-9_]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private void Start()
    {
    }

    [ContextMenu("Test")]
    private async void Test()
    {
        var url = "https://raw.githubusercontent.com/typicode/demo/master/db.json";
        var ct = this.GetCancellationTokenOnDestroy();

        var getFromResources = await GetFromResources();
        var lastModifiedDate = await GetLastModifiedDate(url, ct);

        if (getFromResources > lastModifiedDate)
        {
            Debug.Log($"Local data is newer than remote: {getFromResources} > {lastModifiedDate}");
        }
        else
        {
            Debug.Log($"Remote data is newer or equal: {getFromResources} <= {lastModifiedDate}");
        }

        Debug.Log($"Last-Modified date for {url} is {lastModifiedDate}");
    }

    private async UniTask<DateTime> GetFromResources()
    {
        var lastModifiedString = PlayerPrefs.GetString("level_1_last_modified");
        var lastModified = DateTime.TryParse(lastModifiedString, out var date) ?
            date :
            DateTime.MinValue;

        Debug.Log($"Last-Modified date for level_1 is {lastModified}");

        return lastModified;
    }

    private static async UniTask<DateTime> GetLastModifiedDate(string url, CancellationToken ct)
    {
        var currentLevelIndex = 1;

        var (status, headers) = await HeadAsync(url, 10, ct);

        headers.TryGetValue("Content-Type", out var contentType);
        headers.TryGetValue("Content-Length", out var contentLength);

        headers.TryGetValue("Last-Modified", out var lastModified);
        headers.TryGetValue("ETag", out var etag);

        Debug.Log($"HEAD request to {url} returned status {status}");


        return DateTime.TryParse(lastModified, out var lastModifiedDate) ?
            lastModifiedDate :
            DateTime.MinValue;
    }

    private void UpdateRemoteLevelLastModified(int levelIndex, DateTime lastModified)
    {
        var key = $"level_{levelIndex}_last_modified";
        PlayerPrefs.SetString(key, lastModified.ToString("o")); // ISO 8601 format
        PlayerPrefs.Save();
        Debug.Log($"Updated {key} to {lastModified}");
    }

    public static async UniTask<(long status, Dictionary<string, string> headers)> HeadAsync(
        string url,
        int timeoutSec = 10,
        CancellationToken ct = default,
        IDictionary<string, string> requestHeaders = null)
    {
        using var req = UnityWebRequest.Head(url);
        req.timeout = timeoutSec;

        if (requestHeaders != null)
        {
            foreach (var kv in requestHeaders)
            {
                req.SetRequestHeader(kv.Key, kv.Value);
            }
        }

        await req.SendWebRequest().ToUniTask(cancellationToken: ct);

#if UNITY_2020_1_OR_NEWER
        var ok = req.result == UnityWebRequest.Result.Success;
#else
        bool ok = !(req.isNetworkError || req.isHttpError);
#endif
        if (!ok)
        {
            throw new Exception($"HEAD failed ({req.responseCode}): {req.error}");
        }

        return (req.responseCode, req.GetResponseHeaders());
    }
}