// Assets/_GrandGames/GameModules/Level/Util/LevelJson.cs
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using _GrandGames.GameModules.Level.Domain;

namespace _GrandGames.GameModules.Level.Util
{
    internal static class LevelJson
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Converters = { new StringEnumConverter() } // "hard" -> Difficulty.Hard
        };

        public static LevelData Parse(string json) =>
            string.IsNullOrWhiteSpace(json) ?
                null :
                JsonConvert.DeserializeObject<LevelData>(json, Settings);
    }
}