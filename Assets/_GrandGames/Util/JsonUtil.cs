using System.Text;
using UnityEngine;

namespace _GrandGames.Util
{
    public static class JsonUtil
    {
        public static T FromUtf8<T>(byte[] bytes, int len)
        {
            var json = Encoding.UTF8.GetString(bytes, 0, len);
            return JsonUtility.FromJson<T>(json);
        }

        public static T FromString<T>(string json) => JsonUtility.FromJson<T>(json);
    }
}