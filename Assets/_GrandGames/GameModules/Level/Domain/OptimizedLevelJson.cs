using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace _GrandGames.GameModules.Level.Domain
{
    [Serializable]
    public sealed class OptimizedLevelJson
    {
        [JsonProperty("l")] public int L { get; set; }
        [JsonProperty("li")] public string Li { get; set; }

        [JsonProperty("d")] [JsonConverter(typeof(StringEnumConverter))] public Difficulty D { get; set; }

        [JsonProperty("g")] public int G { get; set; }
        [JsonProperty("mh")] public string Mh { get; set; }
    }
}