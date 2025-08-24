using System;
using System.Text.RegularExpressions;
using _GrandGames.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.GameModules.Level._test
{
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
                // "levels_1_500/level_1_updated" -> otomatk .jsona eklencek
                await LocalFileHelper.CopyStreamingToPersistentAtomic(
                    streamingRelative: "levels_1_500/level_1_updated",
                    persistentRelative: "levels/level_1.json",
                    ct: ct);

                PlayerPrefs.SetString("level_1_last_modified", DateTime.UtcNow.ToString("o")); //TODO: manifest dosyasi olusturulacak veya sqlitea aktarilacak
                PlayerPrefs.Save();
                Debug.Log("level_1 cached (atomic).");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
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
            var s = PlayerPrefs.GetString("level_1_last_modified"); //TODO: manifest dosyasi olusturulacak veya sqlitea aktarilacak
            var ok = DateTime.TryParse(s, out var date);
            var lastModified = ok ?
                date :
                DateTime.MinValue;

            Debug.Log($"Last-Modified date for level_1 is {lastModified:o}");
            return lastModified;
        }
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