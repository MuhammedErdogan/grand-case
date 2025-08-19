using System;
using System.IO;
using System.Text.RegularExpressions;
using _GrandGames.Levels.Logic.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TestNetworkSimulation : MonoBehaviour
{
    //local data path: Assets/_GrandGames/Resources/resources_levels_1_500
    //remote data path: Assets/_GrandGames/Levels/levels_1_500

    private static readonly Regex LevelNameRegex = new("^[a-zA-Z0-9_]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [ContextMenu("GetFromRemote")]
    private async void GetFromRemote()
    {
        var ct = this.GetCancellationTokenOnDestroy();

        try
        {
            var dataAsBytes = await LocalFileHelper.GetStreamingBytes("levels_1_500/level_1_updated", ct: ct);
            var persistentDataPath = Path.Combine(Application.persistentDataPath, "level_1.json");

            await File.WriteAllBytesAsync(persistentDataPath, dataAsBytes, ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [ContextMenu("GetFromResources")]
    private async UniTask<string> GetFromResources()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        var dataFromResources = await LocalFileHelper.GetResourceText("resources_levels_1_500/level_1", ct: ct);

        Debug.Log($"Data from resources: {dataFromResources}");

        return dataFromResources;
    }

    [ContextMenu("GetLastUpdatedTime")]
    private async UniTask<DateTime> GetLastUpdatedTime()
    {
        var lastModifiedString = PlayerPrefs.GetString("level_1_last_modified");
        var lastModified = DateTime.TryParse(lastModifiedString, out var date) ?
            date :
            DateTime.MinValue;

        Debug.Log($"Last-Modified date for level_1 is {lastModified}");

        return lastModified;
    }
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// 0-500 levels
/// 0 --> 10
/// 10 --> 50 ilk 10 leveli discard et, zatne oyandim
/// Algoritma: hangi dilimdeyim? bulundugum dilimdeki next remote 50 leveli al ancak oynaigim kisma kadar olani discard et
/// 25 --> 30 oynadim, oyleyse 25'lik dilimdeyim, 30'a kadar discard et, ilk 50 zaten indilirilmis olabilir o halde indirecegim aralik 50-75
/// Algorithm: [oynadigim level,((bulundugum dilim + 2) * 25)] rangeinde zaten indirilmiş olanlar haric
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// 1-h-8
//