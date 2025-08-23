using System.Collections.Generic;
using UnityEngine;

namespace _GrandGames.Const
{
    public static class UIConst
    {
        //difficulty colors
        public static Dictionary<int, Color> Colors = new()
        {
            { 0, new Color(0.4f, 0.8f, 0.4f) }, // Easy - Green
            { 1, new Color(1f, 0.65f, 0f) }, // Medium - Orange
            { 2, new Color(1f, 0.2f, 0.2f) } // Hard - Red
        };
    }
}