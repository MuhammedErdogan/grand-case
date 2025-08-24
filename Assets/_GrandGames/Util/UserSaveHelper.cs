using UnityEngine;

namespace _GrandGames.Util
{
    public static class UserSaveHelper
    {
        public static void SaveInt(string key, int value) //can send to cloud 
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
    }
}